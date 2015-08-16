﻿using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AirportParser
{
	public class AptWorldwide
	{
		public static void Parser() {
			
			Console.WriteLine ("Warning: this process will take a long time (10+ hours). To stop, close this window or Ctrl/Cmd + C");

			string[] data = System.IO.File.ReadAllLines( @"/Volumes/Hard Drive/Cameron/InfiniteFlight/Airports/apt.mdat");

			for (int i = 0; i < data.Length; i++) {
				string line = data [i];

				if (line.StartsWith ("1 ")) {
					//found start of def

					string icao = line.Substring (15, 4);
					icao = Regex.Replace (icao, @"\s+", "");
					string name = line.Substring (20);

					Console.Write ("Found airport: " + icao + " | " + name + "\n");

					//find country
					string country = CompareToJson (icao);

					//get airport lines
					for (int y = (i + 1); y < data.Length; y++) {

						//search for final line
						string endLine = data [y];

						if (endLine.StartsWith ("1 ")) {
							//found end line

							string[] airport = new List<string> (data).GetRange (i, (y - i)).ToArray ();
							SaveAirport (airport, country, icao);

							break;
						}

					}
						

				}

			}

		}

		static void SaveAirport(string[] airport, string country, string icao) {

			string DirectoryPathRegion = "";

			if (country != null) {

				DirectoryPathRegion = (@"/Volumes/Hard Drive/Cameron/InfiniteFlight/WorldAirports/" + country);

			} else {

				DirectoryPathRegion = (@"/Volumes/Hard Drive/Cameron/InfiniteFlight/WorldAirports/Unknown");

			}

			if(!Directory.Exists(DirectoryPathRegion))
			{
				Directory.CreateDirectory(DirectoryPathRegion);
			}

			//check if ICAO folder exists
			string DirectoryPathAirport = (DirectoryPathRegion + "/" + icao);
			if(!Directory.Exists(DirectoryPathAirport))
			{
				Directory.CreateDirectory(DirectoryPathAirport);
			}

			List<string> NewAirport = new List<string>(); 

			NewAirport.Add("A"); //created on Mac
			NewAirport.Add("1000 Generated by Infinite Flight Airport Editing (World Parser)"); //keep 1000 bit, marks version.
			NewAirport.Add(""); //blank line for spacing

			//run a for loop and assign +3 to compensate for header
			for (int i = 0; i < airport.Length; i++) {

				NewAirport.Add(airport [i]);

				if (i == (airport.Length - 1)) {

					NewAirport.Add ("99");

				}

			}

			string DirectoryToSave = (DirectoryPathAirport + "/apt.dat");

			//write to file
			System.IO.File.WriteAllLines(DirectoryToSave, NewAirport);

		}

		static string CompareToJson(string icao) {

			var fileContents = System.IO.File.ReadAllText (@"/Volumes/Hard Drive/Cameron/InfiniteFlight/Airports/airports.json");
			dynamic json = JsonConvert.DeserializeObject(fileContents);

			//this is a bit slow because it's reloading the file every time... did the job. If you need to regen airports, preload file
			dynamic currentJsonSerialized = JsonConvert.SerializeObject(json[icao]);
			dynamic currentJson = JsonConvert.DeserializeObject(currentJsonSerialized);

			if (currentJson != null) {
				Console.WriteLine ("In country: " + currentJson.country + "\n\n");
				return currentJson.country;
			} else {
				Console.WriteLine ("Country not found\n\n");
				return null;
			}

		}

		public static void UpdateCountryNames() {
			//iterate through folders, compare country name and replace

			string[] directories = Directory.GetDirectories ("/Volumes/Hard Drive/Cameron/InfiniteFlight/WorldAirports/");
			dynamic countries = System.IO.File.ReadAllText (@"/Volumes/Hard Drive/Cameron/InfiniteFlight/Airports/countries.json");

			countries = JsonConvert.DeserializeObject(countries);

			for (int i = 0; i < directories.Length; i++) {
				string directoryFull = directories [i];
				var directory = new DirectoryInfo (directoryFull).Name;
				Console.WriteLine ("Dir: {0}", directory); 


				dynamic serializedJson = JsonConvert.SerializeObject(countries);
				dynamic json = JsonConvert.DeserializeObject(serializedJson);

				foreach (var item in json.Children()) {

					dynamic currentJsonSerial = JsonConvert.SerializeObject(item);
					dynamic currentJson = JsonConvert.DeserializeObject(currentJsonSerial);

					var cca2 = currentJson.cca2.ToString();
					var name = currentJson.name.common.ToString();

					if (directory == cca2) {

						//found actual name
						Console.WriteLine (directory + " is " + name + "\n\n");

						string newPath = directoryFull.Replace (directory, name);
						Directory.Move (directoryFull, newPath);

					}


				}


			}

		}

	}
}

