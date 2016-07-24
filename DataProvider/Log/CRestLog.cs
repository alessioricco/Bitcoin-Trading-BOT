
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Log
{
    public class CRestLog<T> : ABufferedLog<T>
    {
        /// <summary>
        /// Questo flag ci dice se nella precedente operazione c'è stato un errore
        /// </summary>
        private bool HasError = false;
        
        /// <summary>
        /// tipo di richiesta al server
        /// </summary>
        public RestSharp.Method Method { get; set; }

        /// <summary>
        /// url verso quale indirizzare il log
        /// </summary>
        public Uri EndPoint { get; private set; }


        /// <summary>
        /// se true, posso inviare tutti gli elementi nel buffer in una sola request
        /// </summary>
        public bool AggreateSubmissions { get; set; }


        public CRestLog(string url) : base(){
            this.AggreateSubmissions = true;
            this.Method = RestSharp.Method.POST;
            this.EndPoint = new Uri(url);
        }

        protected override void WriteAll()
        {
            var client = new RestSharp.RestClient();

            if (AggreateSubmissions)
            {
                var request = new RestSharp.RestRequest(this.EndPoint, this.Method);
                request.AddBody(this.Buffer);
                client.ExecuteAsync(request, (res, req) => {
                    //se non c'è stato alcun errore, svuota il buffer
                    if (res.ErrorException == null)
                    {
                        this.ClearBuffer();
                    }                    
                });
            }
            else { 
                
                foreach(var item in this.Buffer){

                    var request = new RestSharp.RestRequest(this.EndPoint, this.Method);
                    request.AddBody(item);
                    client.ExecuteAsync(request, (res, req) =>
                    {
                        //se non c'è stato alcun errore, toglie l'elemento dal buffer
                        if (res.ErrorException == null)
                        {
                            this.Buffer.Remove(item);
                        }
                    });
                }

            }

            

        }


        public override void Flush()
        {
            //riscrivo questo metodo, non si deve occupare di cancellare il buffer, ci penseranno i metodi di scrittura
            //base.Flush();

            this.WriteAll();
        }


       
    }
}
