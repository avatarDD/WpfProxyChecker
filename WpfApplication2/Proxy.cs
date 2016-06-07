namespace prxSearcher
{
    class Proxy
    {
        /// <summary>
        /// adress:port
        /// </summary>
        public string adress { get; set; }

        /// <summary>
        /// type of proxy
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// country
        /// </summary>
        public string country { get; set; }

        /// <summary>
        /// latency of proxy
        /// </summary>
        public double latency { get; set; }        
    }
}
