using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Farm.Core.Common
{
    public static class CoreStartParameters
    {
        public static void ExecParams(Configuration configuration,string configPath, params string[] parameters)
        {
            if(parameters == null)
                return;
            foreach (var parameter in parameters)
            {
                switch (parameter)
                {
                    case "detectCard":
                        Console.WriteLine("Detecting cars...");
                        var listCards = new List<VideoCardConfigure>(0);
                        listCards.AddRange(Helper.GetCards()
                            .Select(card => new VideoCardConfigure {Name = card.Card, GUID = $"{card.guid}", Path = card.Path
                            }));

                        if (configuration.Cards.Length > 0 && listCards.Count > 0)
                            Console.WriteLine("Card list not empty, override");

                        var key = Console.ReadLine();
                        if (string.Compare(key, "y", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            configuration.Cards = listCards.ToArray();
                            var newConfig = JsonConvert.SerializeObject(configuration);
                            File.WriteAllText(configPath, newConfig);
                        }
                        break;
                }
            }
        }
    }
}
