// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static ImageViewer.ImageViewerMain;

namespace ImageViewer.Content.Utils
{
    internal class Settings
    {
        //private readonly static string LocalServerUrl = "http://10.10.10.4:8081/";
        private readonly static string NetworkServerUrl = "http://137.135.167.62:8080/";

        private static int ip1 = 10;
        private static int ip2 = 10;
        private static int ip3 = 10;
        private static int ip4 = 4;
        private static int port = 8081;

        private JObject cases = null;
        private JObject imageList = new JObject();
        private JObject imageDetails = new JObject();
        private static bool s_localServer = false;

        internal async Task InitializeAsync()
        {
            cases = await GetJsonAsync("?command=cases");

            if (cases != null && cases.TryGetValue("Cases", out JToken ids))
            {
                Online = true;
                foreach (var id in (ids as JArray))
                {
                    var images = await GetJsonAsync("?command=list&caseID=" + id.Value<string>());
                    imageList.Add(id.Value<string>(), images);
                    if (images.TryGetValue("Images", out JToken names))
                    {
                        foreach (var name in (names as JArray))
                        {
                            var details = await GetJsonAsync("?command=details&caseID=" + id.Value<string>() + "&name="
                                + name.Value<string>());
                            imageDetails.Add(name.Value<string>(), details);
                        }
                    }
                }
            }
        }

        internal static int Mode { get; set; } = 0;

        internal static string URL { get; set; } = NetworkServerUrl;

        internal static string CaseID { get; set; } = "T3461-18";

        internal static string Image1 { get; set; } = "aligned_41.14x_01_aligned__41.14x_01_1_T3502-18_Z4.svs";

        internal static string Image2 { get; set; } = "aligned_41.14x_01_aligned__41.14x_01_1_T3502-18_Z4.svs";

        internal static int Image2offsetX { get; set; } = 0;

        internal static int Image2offsetY { get; set; } = 0;

        internal static int Scaler { get; set; } = 32;

        internal static int Multiplier { get; set; } = 2;

        internal static int MaxResolutionX { get; set; } = 110000;

        internal static int MaxResolutionY { get; set; } = 110000;

        internal static int MinScale { get; set; } = 5;

        internal static int GridX { get; set; } = 4;

        internal static int GridY { get; set; } = 4;

        internal static bool SaveTexture { get; set; } = false;

        internal static bool UseJpeg { get; set; } = true;

        internal static bool DownloadRaw { get; set; } = false;

        internal static bool Online { get; set; } = false;

        internal static void SetIP(Direction direction, int value)
        {
            switch (direction)
            {
                case Direction.UP:      ip1 = value; break;
                case Direction.DOWN:    ip2 = value; break;
                case Direction.LEFT:    ip3 = value; break;
                case Direction.RIGHT:   ip4 = value; break;
                case Direction.FRONT:   port = value; break;
            }

            URL = LocalServer ? LocalServerUrl() : NetworkServerUrl;
        }

        private static string LocalServerUrl()
        {
            return "http://" + ip1.ToString() 
                + "." + ip2.ToString() + "." 
                + ip3.ToString() + "." 
                + ip4.ToString() + ":" 
                + port.ToString() + "/";
        }

        internal static bool LocalServer
        {
            get => s_localServer;
            set
            {
                s_localServer = value;
                URL = value ? LocalServerUrl() : NetworkServerUrl;
            }                      
        }
        internal static string BaseUrl()
        {
            return URL + "?command=image&caseID=" + CaseID + "&name=";
        }

        internal string NextCase(string caseID, bool next)
        {
            if (cases.TryGetValue("Cases", out JToken ids))
            {
                var a = ids as JArray;
                for (var idx = 0; idx < a.Count; idx++)
                {
                    if (caseID.Equals(a[idx].Value<string>()))
                    {
                        return (next ? ((idx + 1) < a.Count ? a[idx + 1] : a.First) :
                                        ((idx - 1) >= 0 ? a[idx - 1] : a.Last)).Value<string>();
                    }
                }
            }
            return caseID;
        }

        internal string FirstImage()
        {
            if (imageList.TryGetValue(CaseID, out JToken list))
            {
                if ((list as JObject).TryGetValue("Images", out JToken images))
                {
                    return (images as JArray).First.Value<string>();
                }
            }
            return null;
        }

        internal string NextImage(string image, bool next)
        {
            if (imageList.TryGetValue(CaseID, out JToken list))
            {
                if ((list as JObject).TryGetValue("Images", out JToken images))
                {
                    var a = images as JArray;
                    for (var idx = 0; idx < a.Count; idx++)
                    {
                        if (image.Equals(a[idx].Value<string>()))
                        {
                            return (next ? ((idx + 1) < a.Count ? a[idx + 1] : a.First) :
                                           ((idx - 1) >= 0 ? a[idx - 1] : a.Last)).Value<string>();
                        }
                    }
                }
            }
            return image;
        }

        internal int Resolution(string image, bool x)
        {
            if (imageDetails.TryGetValue(image, out JToken list))
            {
                if ((list as JObject).TryGetValue("Dimensions", out JToken dimensions))
                {
                    var max = (dimensions as JArray).First.Value<string>();
                    var val = max.Split(',');
                    return Int32.Parse(x ? val[1] : val[2]);
                }
            }
            return 0;
        }

        internal int Offset(string image, bool x)
        {
            if (imageDetails.TryGetValue(image, out JToken list))
            {
                if ((list as JObject).TryGetValue("Offset", out JToken offset))
                {
                    var off = (offset as JValue).Value<string>();
                    var val = off.Split(',');
                    return Int32.Parse(x ? val[0] : val[1]);
                }
            }
            return 0;
        }

        internal int PixelMultiplier(string image)
        {
            if (imageDetails.TryGetValue(image, out JToken list))
            {
                if ((list as JObject).TryGetValue("Dimensions", out JToken dimensions))
                {
                    var a = (dimensions as JArray)[0].Value<string>();
                    var b = (dimensions as JArray)[1].Value<string>();

                    var va = a.Split(',');
                    var vb = b.Split(',');

                    var d = Int32.Parse(va[1]) / Int32.Parse(vb[1]);

                    return d == 4 ? 2 : 1;
                }
            }
            return 1;
        }

        internal int SmallestScale(string image)
        {
            if (imageDetails.TryGetValue(image, out JToken list))
            {
                if ((list as JObject).TryGetValue("Dimensions", out JToken dimensions))
                {
                    var a = (dimensions as JArray).Last.Value<string>();
                    var va = a.Split(',');
                    return Int32.Parse(va[0]);
                }
            }
            return 0;
        }

        internal async Task<JObject> GetJsonAsync(string url)
        {
            var baseUrl = URL + url;
            var request = (HttpWebRequest)WebRequest.Create(baseUrl);
            try
            {
                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var encoding = ASCIIEncoding.ASCII;
                        using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                        {
                            string responseText = reader.ReadToEnd();
                            return JObject.Parse(responseText);
                        }
                    }
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
