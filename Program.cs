/*
Copyright 2015 Joshua Quinn

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in 
compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed 
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for 
the specific language governing permissions and limitations under the License.
*/

/*
 * The program needs to do the following:
 * 
 * 1. Accept input from the console from the user - Done
 * 2. Store the user's input to a variable - Done
 * 3. Pass the user's input stored in the variable to Dell's Warranty API - Done
 * 4. Validate the input against Dell's Warranty API - Done 1/20/2015
 *    a) If validation fails, display the error from the API, and ask for the Service Tag again
 *    b) If validation passes, display the warranty information
 * 
 * Note: I am not sure if Dell's API returns other errors beyond the validation errors. So far, the answer seems to be
 *       no.
 * 
 * TODO: Figure out how to pass country code from the warranty API to get the actual country and display the country
 *       instead of the country code.
 * 
 * Api Keys
 * 1adecee8a60444738f280aad1cd87d0e
 *d676cf6e1e0ceb8fd14e8cb69acd812d
 *849e027f476027a394edd656eaef4842
 */


using System;
using System.Net;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace DellWarranty
{

	class MainClass
	{

		public static void Main (string[] args)
		{

			string st; //service tag
			//string apikey = "1adecee8a60444738f280aad1cd87d0e"; //apikey
			string apikey = "d676cf6e1e0ceb8fd14e8cb69acd812d";

			while (true) {
				//Get Service Tag from Console
				Console.Write ("Enter Dell Service Tag: ");
				st = Console.ReadLine ();

				Console.WriteLine ();

				//Validate cert so that we can pull data from https
				ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback (delegate {
					return true;
				});

				WebClient webClient = new WebClient ();
				string data = string.Empty;
				try {
					data = webClient.DownloadString ("https://api.dell.com/support/v2/assetinfo/warranty/tags.json?svctags=" + st + "&apikey=" + apikey);
					webClient.Dispose ();
				} catch (WebException e) {
					Console.WriteLine (e.Status);
				} catch (Exception e) {
					Console.WriteLine (e.Message);
				}

				JObject jObject = JObject.Parse (data);

				string dataTree = ("GetAssetWarrantyResponse.GetAssetWarrantyResult.Response.DellAsset.");

				JToken faultToken = jObject ["GetAssetWarrantyResponse"] ["GetAssetWarrantyResult"] ["Faults"];

		   /*
			*Machine Description:
			*Service Tag:
			*Ship Date:
			*Country:
			*
			*Service      Provider        Start Date      End Date
			*-------      --------        -----------     ---------
			*contract1    prov1           sdate1          edate1
			*contract2    prov2           sdate2          edate2
			*contract3    prov3           sdate3          edate3
			*
			*/


				if (faultToken.HasValues == false) {

					string machineDescription = (string)jObject.SelectToken (dataTree + "MachineDescription");
					string serviceTag = (string)jObject.SelectToken (dataTree + "ServiceTag");
					DateTime shipDate = (DateTime)jObject.SelectToken (dataTree + "ShipDate");
					string country = (string)jObject.SelectToken (dataTree + "CountryLookupCode");

					Console.WriteLine ("Machine Description: " + machineDescription);
					Console.WriteLine ("Service Tag: " + serviceTag);
					Console.WriteLine ("Ship Date: {0:MM/dd/yyyy HH:mm:ss}", shipDate);
					Console.WriteLine ("Country: " + country);

					Console.WriteLine ();

					Console.WriteLine ("{0,-24} {1,-12} {2,-24} {3,-24}", "Service", "Provider", "Start Date", "End Date");

					foreach (var warranty in jObject["GetAssetWarrantyResponse"]["GetAssetWarrantyResult"]["Response"]["DellAsset"]["Warranties"]["Warranty"]) {
						DateTime warrantyStartDate = (DateTime)warranty ["StartDate"];
						string warrantyService = (string)warranty ["ServiceLevelDescription"];
						string warrantyProvider = (string)warranty ["ServiceProvider"];
						DateTime warrantyEndDate = (DateTime)warranty ["EndDate"];

						Console.WriteLine ("{0,-24} {1,-12} {2,-24:MM/dd/yyyy HH:mm:ss} {3,-24:MM/dd/yyyy HH:mm:ss}", warrantyService, warrantyProvider, warrantyStartDate, warrantyEndDate);

					}
					break;

				} else if (faultToken.HasValues == true) {
					Console.WriteLine(faultToken ["FaultException"] ["Message"]);
					Console.WriteLine ();

				}

			}

         }
	}
}








	

