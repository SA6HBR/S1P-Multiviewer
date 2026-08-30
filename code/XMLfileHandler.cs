using System.Xml.Linq;

namespace S1P_Multiviewer
{
    public class XMLfileHandler
    {
        private static string xmlRoot = "SXP";
        public static void SaveFile(string filePath, IEnumerable<object> favorites = null, bool update = false)
        {
            if (update == false && File.Exists(filePath))
            {
                DialogResult result = MessageBox.Show(
                           "Settingsfile already exists!\nDo you wish to overwrite?",
                           "Warning!",
                           MessageBoxButtons.YesNo,
                           MessageBoxIcon.Warning
                       );

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }
            if (favorites == null)
            {
                var doc = new XDocument(
                        new XElement(xmlRoot,
                            new XElement("Settings",
                                new XElement("Projectname", Parameter.Projectname),
                                //new XElement("ProjectInfo", Parameter.ProjectInfo),
                                new XElement("ProjectInfo", new XCData(Parameter.ProjectInfo ?? "")),
                                new XAttribute("refImp", Parameter.refImp)
                                ),
                            new XElement("Param1",
                                new XElement("Param1Name", Parameter.Param1Name)
                                ),
                            new XElement("Param2",
                                new XElement("Param2Name", Parameter.Param2Name)
                                ),
                            new XElement("Param3",
                                new XElement("Param3Name", Parameter.Param3Name)
                                ),
                            new XElement("Param4",
                                new XElement("Param4Name", Parameter.Param4Name)
                                )
                            )
                        );
                doc.Save(filePath);
            }
            else
            {
                var doc = new XDocument(
                        new XElement(xmlRoot,
                            new XElement("Settings",
                                new XElement("Projectname", Parameter.Projectname),
                                //new XElement("ProjectInfo", Parameter.ProjectInfo),
                                new XElement("ProjectInfo", new XCData(Parameter.ProjectInfo ?? "")),
                                new XAttribute("refImp", Parameter.refImp)
                                ),
                            new XElement("Param1",
                                new XElement("Param1Name", Parameter.Param1Name)
                                ),
                            new XElement("Param2",
                                new XElement("Param2Name", Parameter.Param2Name)
                                ),
                            new XElement("Param3",
                                new XElement("Param3Name", Parameter.Param3Name)
                                ),
                            new XElement("Param4",
                                new XElement("Param4Name", Parameter.Param4Name)
                                ),
                                 new XElement("Favorites",
                                        favorites.Select(favorite =>
                                            new XElement("Favorite", favorite.ToString()))
                                    )
                            )
                        );
                doc.Save(filePath);
            }
        }

        public static void ReadFile(string filePath, ListBox listBox)
        {
            if (File.Exists(filePath))
            {
                XDocument doc = XDocument.Load(filePath);

                var XMLPart = doc.Descendants(xmlRoot).Elements("Settings");
                Parameter.Projectname = XMLPart.Elements("Projectname").Count() > 0 ? XMLPart.Elements("Projectname").First().Value.ToLower() : Parameter.Projectname;
                Parameter.ProjectInfo = XMLPart.Elements("ProjectInfo").Count() > 0 ? XMLPart.Elements("ProjectInfo").First().Value.Replace("\n", Environment.NewLine) : Parameter.ProjectInfo;
                Parameter.refImp = XMLPart.Attributes("refImp").Count() > 0 ? int.Parse(XMLPart.Attributes("refImp").First().Value) : Parameter.refImp;

                XMLPart = doc.Descendants(xmlRoot).Elements("Param1");
                Parameter.Param1Name = XMLPart.Elements("Param1Name").Count() > 0 ? XMLPart.Elements("Param1Name").First().Value : Parameter.Param1Name;

                XMLPart = doc.Descendants(xmlRoot).Elements("Param2");
                Parameter.Param2Name = XMLPart.Elements("Param2Name").Count() > 0 ? XMLPart.Elements("Param2Name").First().Value : Parameter.Param2Name;

                XMLPart = doc.Descendants(xmlRoot).Elements("Param3");
                Parameter.Param3Name = XMLPart.Elements("Param3Name").Count() > 0 ? XMLPart.Elements("Param3Name").First().Value : Parameter.Param3Name;

                XMLPart = doc.Descendants(xmlRoot).Elements("Param4");
                Parameter.Param4Name = XMLPart.Elements("Param4Name").Count() > 0 ? XMLPart.Elements("Param4Name").First().Value : Parameter.Param4Name;


                XMLPart = doc.Descendants(xmlRoot).Elements("Favorites");
                listBox.Items.Clear();

                foreach (var favorite in XMLPart.Elements("Favorite"))
                {
                    listBox.Items.Add(favorite.Value);
                }
            }
            else
            {
                SaveFile(filePath);
            }

        }

    }
}
