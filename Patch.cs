/* Code in this file is thanks to nesrak1
 * https://github.com/nesrak1/AddressablesTools/blob/master/Example/Program.cs#L52-L93
 */

using AddressablesTools;
using AddressablesTools.Catalog;
using System.Text.Json;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;

namespace Concursus
{
    class Patch
    {
        public static void PatchCRC(string input, string output)
        {
            ContentCatalogData ccd = AddressablesJsonParser.FromString(File.ReadAllText(input));

            foreach (var resourceList in ccd.Resources.Values)
            {
                foreach (var rsrc in resourceList)
                {
                    if (rsrc.ProviderId == "UnityEngine.ResourceManagement.ResourceProviders.AssetBundleProvider")
                    {
                        var data = rsrc.Data;
                        if (data != null && data is ClassJsonObject classJsonObject)
                        {
                            JsonSerializerOptions options = new JsonSerializerOptions()
                            {
                                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                            };

                            JsonObject? jsonObj = JsonSerializer.Deserialize<JsonObject>(classJsonObject.JsonText);
                            if (jsonObj != null)
                            {
                                jsonObj["m_Crc"] = 0;
                                classJsonObject.JsonText = JsonSerializer.Serialize(jsonObj, options);
                                rsrc.Data = classJsonObject;
                            }
                        }
                    }
                }
            }

            File.WriteAllText(output, AddressablesJsonParser.ToJson(ccd));
        }
    }
}
