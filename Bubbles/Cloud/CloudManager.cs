using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.Cloud
{
    /// <summary>
    /// offre un'interfaccia unica per dialogare con i servizi cloud
    /// </summary>
    class CloudManager
    {


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sessionToken">identificativo della sessione corrente</param>
        /// <param name="collection">nome della collezione alla quale i dati fanno riferimento</param>
        /// <param name="data">oggetto contentente i dati</param>
        public static void Push<T>(string sessionToken, string collection,  T data){

            try
            {
                var host = MainConfiguration.Configuration.CloudHost;
                var url = string.Format("{0}/s/{1}/{2}", host, sessionToken, collection);

                var rest = new DataProvider.Log.CRestLog<T>(url)
                {
                    AggreateSubmissions = false,
                    BufferSize = -1,
                    Enabled = true,
                    Method = RestSharp.Method.POST
                };

                rest.Log(data);
            }
            catch (Exception ex)
            {
                //dobbiamo tracciarci gli errori
                // per ora questa funzionalità non è bloccante
                
            }

        }


    }
}
