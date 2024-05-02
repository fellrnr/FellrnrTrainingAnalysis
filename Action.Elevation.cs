using GoogleApi.Entities.Maps.Elevation.Request;
using GoogleApi;
using GoogleApi.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;
using MemoryPack;

namespace FellrnrTrainingAnalysis.Action
{

    public class Elevation
    {

        public Elevation()
        {

        }

        public async Task<TimeSeriesBase?> GetElevation(LocationStream locationStream, Activity activity)
        {
            string path = Path.Combine(Options.AppDataPath, $"ElevationData_{activity.PrimaryKey()}.mp");

            TimeSeriesBase? result = null;

            if (File.Exists(path))
            {
                try
                {
                    byte[] bin = File.ReadAllBytes(path);

                    result = MemoryPackSerializer.Deserialize<TimeSeriesBase>(bin);
                    Logging.Instance.Debug($"Derialized {path} for elevation");
                    return result;
                }
                catch 
                {
                    Logging.Instance.Error($"Derialization failed on {path}");
                    throw new Exception($"Derialization failed on {path}"  );
                }
            }

            if (Options.Instance.UseGoogleApi)
            {
                ElevationGoogle elevationGoogle = new ElevationGoogle();
                result = await elevationGoogle.GetElevation(locationStream, activity);
            }
            else
            {
                ElevationOpenAPI elevationOpenAPI = new ElevationOpenAPI();
                result = await elevationOpenAPI.GetElevation(locationStream, activity);
            }

            if (result != null)
            {
                using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var bin = MemoryPackSerializer.Serialize(result);
                    stream.Write(bin);
                }
            }
            else
            {
                Logging.Instance.Error($"No result from getting elevation");
            }

            return result;
        }
    }

    public class ElevationGoogle 
    {

        public async Task<TimeSeriesBase?> GetElevation(LocationStream locationStream, Activity activity)
        {
            List<float> elevations = new List<float>();
            float prev = 0;
            bool first = true;
            int chunk = Options.Instance.GoogleApiChunk;
            try
            {
                for (int i = 0; i < locationStream.Latitudes.Length; i += chunk)
                {
                    List<Coordinate> coordinates = new List<Coordinate>();

                    int limit = Math.Min(locationStream.Longitudes.Length, i + chunk);
                    for (int j = i; j < limit; j++)
                    {
                        var coordinate = new Coordinate(latitude: locationStream.Latitudes[j], longitude: locationStream.Longitudes[j]);
                        coordinates.Add(coordinate);
                    }
                    var request = new ElevationRequest
                    {
                        Key = Options.Instance.GoogleApiKey,
                        Locations = coordinates,
                    };

                    var response = await GoogleMaps.Elevation.QueryAsync(request);

                    foreach (var elevationResponse in response.Results)
                    {
                        if (first && elevationResponse.Location.Latitude != locationStream.Latitudes[i])
                        {
                            Logging.Instance.Log($"Location Latitude mismatch {elevationResponse.Location.Latitude}, {locationStream.Latitudes[i]}");
                            first = false;
                        }
                        if (first && elevationResponse.Location.Longitude != locationStream.Longitudes[i])
                        {
                            Logging.Instance.Log($"Location Longitude mismatch {elevationResponse.Location.Longitude}, {locationStream.Longitudes[i]}");
                            first = false;
                        }
                        if (elevationResponse.Elevation == null)
                        {
                            elevations.Add(prev);
                        }
                        else
                        {
                            elevations.Add((float)elevationResponse.Elevation);
                            prev = (float)elevationResponse.Elevation;
                        }
                    }

                }


                //location stream isn't 1 second (yet)
                TimeSeriesRecorded timeSeriesRecorded = new TimeSeriesRecorded(Activity.TagAltitude, TimeValueList.TimeValueListFromTimed(locationStream.Times!, elevations.ToArray()), activity);
                return timeSeriesRecorded;
            }
            catch (Exception e)
            {
                Logging.Instance.Error($"Exception {e.Message} in CallElevationRestApi");
            }
            return null;

        }
    }

    //useful
    //https://devblogs.microsoft.com/pfxteam/await-and-ui-and-deadlocks-oh-my/
    public class ElevationOpenAPI
    {
        const string REST_URI = "api/v1/lookup";
        const string REST_API = "https://api.open-elevation.com";

        static HttpClient client = new HttpClient();// HttpClientFactory.Create(new LoggingHandler());
        //static HttpClient client = HttpClientFactory.Create(new LoggingHandler());

        public async Task<TimeSeriesBase?> GetElevation(LocationStream locationStream, Activity activity)
        {
            TimeSeriesBase? result = await CallElevationRestApi(locationStream, activity);

            return result;
        }

        //const int LIMIT = 2;

        async Task<TimeSeriesBase?> CallElevationRestApi(LocationStream locationStream, Activity activity)
        {
            client.BaseAddress = new Uri(REST_API);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                List<LocationMessage> locations = new List<LocationMessage>();
                //for(int i=0 ; i< locationStream.Latitudes.Length && i < LIMIT; i++)
                for (int i = 0; i < locationStream.Latitudes.Length; i++)
                {
                    LocationMessage location = new LocationMessage(latitude: locationStream.Latitudes[i], longitude: locationStream.Longitudes[i]);
                    locations.Add(location);
                }


                RequestMessage request = new RequestMessage(locations);
                HttpResponseMessage response = await client.PostAsJsonAsync(REST_URI, request);

                if (!response.IsSuccessStatusCode)
                {
                    Logging.Instance.Error($"Bad response code {response.StatusCode}");
                    return null;
                }
                HttpContent content = response.Content;
                //additional package:  Microsoft.AspNet.WebApi.Client
                ResponseMessage? result = await content.ReadAsAsync<ResponseMessage>();
                if (result == null || result.results == null || result.results.Length == 0)
                {
                    Logging.Instance.Error("Got no results back");
                    return null;
                }
                float[] elevations = new float[result.results.Length];
                for(int i=0; i <  result.results.Length; i++)
                {
                    elevations[i] = result.results[i].elevation;
                }

                //location stream isn't 1 second (yet)
                TimeSeriesRecorded timeSeriesRecorded = new TimeSeriesRecorded(Activity.TagAltitude, TimeValueList.TimeValueListFromTimed(locationStream.Times!, elevations), activity);
                return timeSeriesRecorded;
            }
            catch (Exception e)
            {
                Logging.Instance.Error($"Exception {e.Message} in CallElevationRestApi");
            }
            return null;
        }



        public class RequestMessage
        {
            public RequestMessage(List<LocationMessage> locations)
            {
                this.locations = locations;
            }

            public List<LocationMessage> locations { get; set; }
        }

        public class LocationMessage
        {
            public LocationMessage(float latitude, float longitude)
            {
                this.latitude = latitude;
                this.longitude = longitude;
            }

            public float latitude { get; set; }
            public float longitude { get; set; }
        }

        public class ResponseMessage
        {
            public ResponseMessage(ElevationMessage[] elevation)
            {
                this.results = elevation;
            }

            public ElevationMessage[] results { get; set; }
        }


        public class ElevationMessage
        {
            public float latitude { get; set; }
            public float longitude { get; set; }
            public float elevation { get; set; }
        }
        class LoggingHandler : DelegatingHandler
        {

            public LoggingHandler()
            {
            }

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                string? raw = null;
                if (request.Content != null)
                {
                    HttpContent content = request.Content;
                    raw = await content.ReadAsStringAsync();

                    Utils.Logging.Instance.Error($"raw request [{raw}]");
                }
                var response = await base.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    Utils.Logging.Instance.Error($"HTTP Error, RequestUri {request.RequestUri}, StatusCode {response.StatusCode}");
                }
                else
                {
                    string rawres = await response.Content.ReadAsStringAsync();
                    Utils.Logging.Instance.Debug($"Raw result [{rawres}]");
                }
                return response;
            }

        }

    }
}
