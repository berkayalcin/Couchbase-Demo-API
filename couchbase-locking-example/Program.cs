using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Authentication;
using Couchbase.Configuration.Client;
using Couchbase.Core;

namespace Couchbase_Lock_Demo
{
    class Program
    {
        private static IBucket _bucket;

        static async Task Main(string[] args)
        {
            await InitializeBucket();
            StartOptimisticLocking();
        }

        private static async Task InitializeBucket()
        {
            var cluster = new Cluster(new ClientConfiguration
            {
                Servers = new List<Uri>
                {
                    new("http://localhost:8091")
                }
            });

            var authenticator = new PasswordAuthenticator("Administrator", "verystrongpassword");
            cluster.Authenticate(authenticator);
            _bucket = await cluster.OpenBucketAsync("travel-sample");
        }

        #region Optimistic Locking CAS

        private static void StartOptimisticLocking()
        {
            new Thread(CasMismatchUnhandled).Start();
            new Thread(CasHappyPath).Start();
            new Thread(CasMismatchHandled).Start();
        }

        private static void CasHappyPath()
        {
            const string airlineId = "airline_10";
            var result = _bucket.Get<Airline>(airlineId);
            var newDocument = result.Value;
            newDocument.Country = "France";
            newDocument.Icao = "TXW";
            var currentCas = result.Cas;
            var newResult = _bucket.Replace(airlineId, newDocument, cas: currentCas);
            Console.WriteLine(
                $"Happy Path : Current Cas {currentCas} Operation Result Success");
        }

        private static void CasMismatchUnhandled()
        {
            const string airlineId = "airline_10";
            var result = _bucket.Get<Airline>(airlineId);
            Thread.Sleep(3500);
            var newDocument = result.Value;
            newDocument.Iata = "TQ";
            newDocument.Country = "Egypt";
            var currentCas = result.Cas;
            var newResult = _bucket.Replace(airlineId, newDocument, cas: currentCas);
            Console.WriteLine($"Unhandled Mismatch : Current Cas {currentCas} Operation Result {newResult.Message}");
        }

        private static void CasMismatchHandled()
        {
            const string airlineId = "airline_10";
            while (true)
            {
                var result = _bucket.Get<Airline>(airlineId);
                Thread.Sleep(1000);
                var newDocument = result.Value;
                newDocument.Icao = "MLA";
                newDocument.Count = 3;
                var currentCas = result.Cas;
                var newResult = _bucket.Replace(airlineId, newDocument, cas: currentCas);
                if (newResult.Success)
                {
                    Console.WriteLine("Handled Mismatch Is Successfully Ended");
                    break;
                }

                if (!newResult.Success && newResult.Exception is CasMismatchException casMismatchException)
                {
                    Console.WriteLine(
                        $"Handled Mismatch Will Retry Again : Current Cas {currentCas} {casMismatchException.Message}");
                }
            }
        }

        #endregion
    }
}