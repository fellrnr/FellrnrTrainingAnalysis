# FellrnrTrainingAnalysis

This software is still under development, so functionality is limited. You are welcome to take the code and use it as the base for your own development. 

The goal is to create a flexible platform that will support plugins and extensions, in much the same way that SportTracks 3 did before it was discontinued. The code is intended to be simple and easy to understand, avoiding anything 'clever'. It also trys to avoid knowing anything about the data, and use configuration and introspection where possible. 

Currently, the code will import a Strava data export. Unzip the export, then import the profile.csv file. This will then pull in the activites and parse the FIT files.

You can also sync with Strava, though you'll have to create your own API keys. 

# ObjectListView

Note that this project includes a fork of ObjectListView, as I needed to get it working with .NET core. I took https://github.com/FizzcodeSoftware/ObjectListViewRepack and made some changes marked with "//#!JFS!#". I'm hoping the main trunk of OLV will soon support .NET core and I can delete this fork. 
