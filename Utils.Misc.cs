using System.IO.Compression;

namespace FellrnrTrainingAnalysis.Utils
{
    public class Misc //the class of misfit toys
    {
        public static Stream DecompressAndOpenFile(string filename)
        {
            //we have to unzip the .gz files strava gives us to a temp file. Using the GZFileStream doesn't work as the FIT toolkit seeks around, which GZ doesn't support

            if (filename.ToLower().EndsWith(".gz"))
            {
                string newfile = filename.Remove(filename.Length - 3);
                if (!File.Exists(newfile))
                {
                    using FileStream compressedFileStream = File.Open(filename, FileMode.Open);
                    using FileStream outputFileStream = File.Create(newfile);
                    using var decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress);
                    decompressor.CopyTo(outputFileStream);
                }
                filename = newfile;
            }

            FileStream fitSource = new FileStream(filename, FileMode.Open, FileAccess.Read);
            return fitSource;

        }
    }
}
