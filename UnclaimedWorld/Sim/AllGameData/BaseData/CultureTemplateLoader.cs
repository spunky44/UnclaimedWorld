using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities.Templates;

namespace UWGame.SimSide.AllGameData
{
    public class CultureTemplateLoader
    {
        public static List<CultureTemplate> Init()
        {
            List<CultureTemplate> list = new List<CultureTemplate>();
            #region Descendant culture 1 (Fields of Tau Ceti)
            /////////////MALE
            #region maleDescendantWhiteCulture
            list.Add(new CultureTemplate()
                {
                     KeyName = "maleDescendantWhiteCulture",
                     AgeInYears = new NormalDistribution() { Min = 20f, Max = 60f }, 
                     CasteKey = "male",
                    //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                     RaceKey = "whiteHumanDescendant", //
                     CommonFirstNames = new[] { "John", "Jon",  "Jim",  "Walt", "Roger", "Niel", "Andon", "Paul", "Steven", "Samuel", "Marek", "Jesper", "Jens", "Red", "Tyson", "Bradley", "Anders", "Mark", "Lau"   },
                 //    UncommonFirstNames = new[] { "Walt", "Roger", "Neil", "Andon", "Paul", "Steven" },
                     CommonLastNames = new[] { "Green", "Beaumont", "Nyman", "Morozov", "Rence", "Reikhart", "Vaidman", "Robinson", "Roux", "Durand", "Welley", "Clark", "Beck", "Cramarr", "Vasilev", "Zahnder", "Gelaesen", "Zholud", "Stefan", "Runar", "Vonseiten", "Pennington", "Lloyd", "Hansson" },
                  //   UncommonLastNames = new[] { "Beaumont" }
                });
            #endregion
            #region maleDescendantAsianCulture
            list.Add(new CultureTemplate()
            {
                KeyName = "maleDescendantAsianCulture",
                AgeInYears = new NormalDistribution() { Min = 20f, Max = 60f },
                CasteKey = "male",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "asianHumanDescendant", //
                CommonFirstNames = new[] { "Timur", "Hiroto", "Ren", "Sho", "Michael", "Nick", "John", "Dean", "Razvan", "Chris", "Julian", "Coby", "Ike", "Marcus", },
             //   UncommonFirstNames = new[] { "Walt", "Roger", "Neil" },
                CommonLastNames = new[] { "Saito", "Bashi", "Inoue", "Mani", "Weng", "Mellon", "Tuke" },
             //   UncommonLastNames = new[] { "Beaumont" }
            });
            #endregion
            #region maleDescendantHispanicCulture
            list.Add(new CultureTemplate()
            {
                KeyName = "maleDescendantHispanicCulture",
                AgeInYears = new NormalDistribution() { Min = 20f, Max = 60f },
                CasteKey = "male",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "hispanicHumanDescendant", //
                CommonFirstNames = new[] { "Ruben", "Tau", "Nathan", "Amer", "Sebastin", "Matas", "Daniel", "David", "Nicky", "Emanuel", "Mike", "Enzo", "Ziki" },
              //  UncommonFirstNames = new[] { "Walt", "Roger", "Neil" },
                CommonLastNames = new[] { "Sica", "Gutierrez", "Manrique", "Vargas", "Manes", "Fernandez", "Jimenez" },
          //      UncommonLastNames = new[] { "Beaumont" }
            });
            #endregion
            #region maleDescendantBlackCulture
            list.Add(new CultureTemplate()
            {
                KeyName = "maleDescendantBlackCulture",
                AgeInYears = new NormalDistribution() { Min = 20f, Max = 60f },
                CasteKey = "male",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "blackHumanDescendant", //
                CommonFirstNames = new[] { "Kato", "Christian", "John", "Daman", "Will", "Sekan", "Garai", "Robert", "Joseph", "Jonathan", "Pyre", "Marc", "Etienne", "Jacob" },
            //    UncommonFirstNames = new[] { "Walt", "Roger", "Neil" },
                CommonLastNames = new[] { "Williams", "Thomas", "Girard", "Marost", "David", "Tiler", "Ka", "Anderson", "Powers" },
             //   UncommonLastNames = new[] { "Beaumont" }
            });
            #endregion
/////////////FEMALE
            #region femaleDescendantWhiteCulture
            list.Add(new CultureTemplate()
            {
                KeyName = "femaleDescendantWhiteCulture",
                AgeInYears = new NormalDistribution() { Min = 20f, Max = 60f },
                CasteKey = "female",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "whiteHumanDescendant", //
                NoOfPortraitFlavours = 2, // NEW, there are 2 portraits to choose from
                CommonFirstNames = new[] { "Cherea", "Thera", "Tine", "Julia", "Nisa", "Petia", "Chlo", "Mari", "Sabella", "Eres", "Lyne", "Elin", "Rena", "Kimby", "Julie", "Jan" }, // "Linsey" used as character person
            //    UncommonFirstNames = new[] { "Walt", "Roger", "Neil" },
                CommonLastNames = new[] { "Smith", "Vartanian", "Sokolof", "Wilson", "Moore", "Broun", "Harrys", "Kozlov", "Morel", "Katyna", "Elitz", "Huse", "Sky", "Hart" , "Sullivan", "Zimmer", "van Veen"},
            //    UncommonLastNames = new[] { "Beaumont" }
            });
            #endregion 
            #region femaleDescendantBlackCulture
            list.Add(new CultureTemplate()
            {
                KeyName = "femaleDescendantBlackCulture",
                AgeInYears = new NormalDistribution() { Min = 20f, Max = 60f },
                CasteKey = "female",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "blackHumanDescendant", //
                CommonFirstNames = new[] { "Gabrielle", "Jane", "Julia", "Ema", "Linda", "Eva", "Mara", "Jessa", "Patricia", "Valeri" },
         //       UncommonFirstNames = new[] { "Walt", "Roger", "Neil" },
                CommonLastNames = new[] { "Michel", "Clark", "Laurent", "Isah", "Tores" ,"Artis", "Fair", "Ganda", "Gauvin", "Richard" }, //"Cattier" used as character person
          //      UncommonLastNames = new[] { "Beaumont" }
            });
            #endregion

            #region dogCulture
            list.Add(new CultureTemplate()
            {
                KeyName = "dogCulture",
                AgeInYears = new NormalDistribution() { Min = 2f, Max = 5f },               
                CommonFirstNames = new[] { "Rover", "Max", "Buster" } 
               
            });
            #endregion 
            #endregion

//////////////////////////////////////////////////////////////////////////
            #region Descendant culture 2 (Headway)
            /////////////MALE
            #region maleDescendantWhiteCulture2
            list.Add(new CultureTemplate()
            {
                KeyName = "maleDescendantWhiteCulture2",
                AgeInYears = new NormalDistribution() { Min = 18f, Max = 68f },
                CasteKey = "male",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "whiteHumanDescendant", //
                CommonFirstNames = new[] { "John", "Celoy", "Jim", "Walt", "Cyrus", "Alex",  "Paul", "Steven", "Samuel", "Adrian", "Costin", "Paul", "Jake", "Jasper", "Kirk", "Leonard",   "Miles", "Nat", "Oliver", "Randy", "Sid", "Theo" },
                //    UncommonFirstNames = new[] { "Walt", "Roger", "Neil", "Andon", "Paul", "Steven" },
                CommonLastNames = new[] { "Millet", "Walker", "Perroh", "Brayman", "Langsdor", "Henze", "Nelson", "Goan", "Pierce", "Late", "Nord", "Fritt", "Rojartz", "Lavin", "Kaspar", "Novak", "Banik", "Tesar", "Simonis", "Dalca", "Vasile", "Baudin", "Green", "Beaumont", "Tash", "Tod", "Traviss", },
                //   UncommonLastNames = new[] { "Beaumont" }
            });
            #endregion
            #region maleDescendantAsianCulture2
            list.Add(new CultureTemplate()
            {
                KeyName = "maleDescendantAsianCulture2",
                AgeInYears = new NormalDistribution() { Min = 18f, Max = 68f },
                CasteKey = "male",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "asianHumanDescendant", //
                CommonFirstNames = new[] { "Adi", "Tri", "Raja", "Sang", "Hyun", "Ren", "Sho",  "Nick", "John", "Dean", "Jim", "Chris", "Coby", "Ike", "Marcus", "Michael", },

                CommonLastNames = new[] { "Kim", "Lee", "Jiang", "Sato", "Zhou", "Ruan", "Darzi", "Joshi" },

            });
            #endregion
            #region maleDescendantHispanicCulture2
            list.Add(new CultureTemplate()
            {
                KeyName = "maleDescendantHispanicCulture2",
                AgeInYears = new NormalDistribution() { Min = 18f, Max = 68f },
                CasteKey = "male",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "hispanicHumanDescendant", //
                CommonFirstNames = new[] { "Adam", "Luca", "Nathan", "Amer", "Sebastin", "Matas", "Daniel", "David", "Aurel", "Emanuel", "Mike", "Enzo", "Sorin", "Victor" },
                //  UncommonFirstNames = new[] { "Walt", "Roger", "Neil" },
                CommonLastNames = new[] { "Villa", "Vargas", "Sastre", "Rana", "Gebara", "Mendoza", "Viteri", "Aritza", "Ferrer", "Roig" },
                //      UncommonLastNames = new[] { "Beaumont" }
            });
            #endregion
            #region maleDescendantBlackCulture2
            list.Add(new CultureTemplate()
            {
                KeyName = "maleDescendantBlackCulture2",
                AgeInYears = new NormalDistribution() { Min = 18f, Max = 68f },
                CasteKey = "male",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "blackHumanDescendant", //
                CommonFirstNames = new[] { "Kofi", "Amos", "Christian", "John", "Akan", "Will", "Sekan", "Akachi", "Robert", "Joseph", "Jerome", "Chidi", "Marc", "Kojo", "Jacob", "Isiah", "Jamie", "Udo","Lewis", "Mack", },
                //    UncommonFirstNames = new[] { "Walt", "Roger", "Neil" },
                CommonLastNames = new[] { "Williams", "Thomas", "Tash", "Stacks", "Howe", "Ivers", "Ka", "Anderson", "Powers", "Laurent", },
                //   UncommonLastNames = new[] { "Beaumont" }
            });
            #endregion
            /////////////FEMALE   ..make sure females have the same total amount of first names to choose from as the males
            #region femaleDescendantWhiteCulture2
            list.Add(new CultureTemplate()
            {
                KeyName = "femaleDescendantWhiteCulture2", //mp I put a few of the names from the male other cultures here as well, to indicate kinship.
                AgeInYears = new NormalDistribution() { Min = 18f, Max = 68f },
                CasteKey = "female",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "whiteHumanDescendant", //
                NoOfPortraitFlavours = 2, // NEW, there are 2 portraits to choose from
                CommonFirstNames = new[] { "Corina", "Emilia", "Irina", "Liana", "Lidia", "Luisa", "Mirela", "Mari", "Sabella", "Eres", "Lyne", "Elin", "Rena", "Kimby", "Julie", "Jan", "Edina", "Dara", "Sophea", "Chea", "Chan", "Stela", "Magda", "Miruna", "Ramona", "Alise", "Christie", "Emmie", "Karin", "Kirsten", "Laura", "Leona", "Paula", "Tara", "Vera" }, // 
                //    UncommonFirstNames = new[] { "Walt", "Roger", "Neil" },
                CommonLastNames = new[] { "Pierce", "Late", "Nord", "Fritt", "Rojartz", "Lavin", "Kaspar", "Novak", "Banik", "Tesar", "Simonis", "Dalca", "Vasile",  "Foss", "Giles", "Mullins", "Travere",   "Viteri", "Aritza" },
                //    UncommonLastNames = new[] { "Beaumont" }
            });
            #endregion
            #region femaleDescendantBlackCulture2
            list.Add(new CultureTemplate()
            {
                KeyName = "femaleDescendantBlackCulture2",
                AgeInYears = new NormalDistribution() { Min = 18f, Max = 68f },
                CasteKey = "female",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "blackHumanDescendant", //
                CommonFirstNames = new[] { "Sarah", "Taylis", "Julia", "Ana", "Linda", "Eva", "Mara", "Sanda", "Kiri", "Valeri", "Amara", "Adanna", "Liza", "Alea", "Alyx", "Becca", "Brook", "Callista", "Cassie", "Christa", "Cora", "Dani", "Elea", "Elise", "Gail", "Erika", "Gemma", "Gillian", "Helena", "Ina", "Janey", "Jeannie", "Kelly", "Lana", "Reene", "Serena" },
                //       UncommonFirstNames = new[] { "Walt", "Roger", "Neil" },
                CommonLastNames = new[] { "Michel", "Clark", "Laurent", "Artis", "Fair", "Wade", "Wray", "Rains", "Reier", "Sadler", "Allard", "Tash" }, //
                //      UncommonLastNames = new[] { "Beaumont" }
            });
            #endregion
            #endregion
///////////////////////////////////////////////////////////////////////////
            #region Planetfall culture , ancestors (Muckroot station)
            //the names here are less mixed and should be from more well-defined countries.
            /////////////MALE
            #region malePlanetfallWhiteAngloCulture
            list.Add(new CultureTemplate()
            {
                KeyName = "malePlanetfallWhiteAngloCulture",
                AgeInYears = new NormalDistribution() { Min = 18f, Max = 68f },
                CasteKey = "male",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "whiteHumanAncestor", //
                CommonFirstNames = new[] { "Thomas", "John", "Jim", "Walt", "Alex", "Axel", "Andrew", "Aaron", "Paul", "Steven", "Paul", "Jake", "Jasper", "Christopher", "Chris", "Carl", "Case", "Clay", "Jay", "Jeremy", "Rick", "Stevie", "Dave", "Douglas", "Edward", "Eric", "Frank", "Rich", "Robin", "Ronny", "Scott", "Shane", "Simon", "Ted", "Matthew", "Charlie", "Noah", "Jack", "James",  },
                UncommonFirstNames = new[] {"Arthur", "Cyrus", "Avery", "Austin", "Samuel", "Adrian", "Theo", "Kirk", "Leonard", "Miles", "Oliver", "Randy", "Sid", "Brendan", "Bruce", "Buzz", "Carter", "Conrad", "Heath", "Morgan", "Reynold", "Rodney", "Timothy", "Darrell", "Garrett", "Gerard", "Glenn", "Grant", "Greg", "Hal", "Harvey", "Henry", "Hugh", "Hugo", "Irving", "Jeff", "Jeffrey", "Jeremiah", "Jesse", "Jimmy", "Joel", "Julian", "Keith", "Kenneth", "Kiefer", "Kyle", "Lance", "Lawrence", "Lou", "Luke", "Matt", "Max", "Milo", "Mitch", "Nat", "Nate", "Nash", "Neil", "Niles", "Oliver", "Owen", "Philip", "Quinn", "Ralph", "Randall", "Raymond", "Reggie", "Reuben", "Emmett", "Timmy", "Victor", "Walter", "Wilson" },
                CommonLastNames = new[] { "Walker", "Brayman", "Nelson", "Nielsen", "Goan", "Pierce", "Late", "Fritt", "Lavin", "Kaspar", "Novak", "Beaumont", "Vasile", "Baudin", "Green", "Beaumont", "Tash", "Tod", "Traviss", "Smith", "Johnson", "Williams", "Brown", "Jones", "Miller", "Davis", "Wilson", "Thomas", "Moore", "Martin", "Jackson", "Thompson", "White", "Lee", "Harris", "Clark", "Lewis", "Robinson" },
                //   UncommonLastNames = new[] { "Beaumont" }
            });
            #endregion
            #region malePlanetfallRussianCulture
            list.Add(new CultureTemplate()
            {
                KeyName = "malePlanetfallWhiteRussianCulture",
                AgeInYears = new NormalDistribution() { Min = 18f, Max = 68f },
                CasteKey = "male",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "whiteHumanAncestor", //
                CommonFirstNames = new[] { "Piotr", "Alexander", "Maxim", "Ivan", "Artyom", "Dmitry", "Mikhail", "Nikita", "Daniil", "Yegor", "Andrei", "Viktor", "Boris", "Dmitry", "Pavel", "Ruslan", "Feliks", "Denis", "Gennadi", "Isaak", "Kazimir", "Kiril", "Konstantin", "Lazar", "Maxim", "Sergey", "Stephan", "Timur", "Vadim", "Vlad", "Vladimir", "Yanick", "Yevgeni", "Zakhar" },

                CommonLastNames = new[] { "Smirnov", "Ivanov", "Kuznetsov", "Popov", "Sokolov", "Lebedev", "Kozlov", "Novikov", "Morozov", "Petrov", "Volkov", "Vasilyev", "Zaytsev", "Yazova", "Zaytsev", "Timmerman", "Stasov", "Tikhonov" },

            });
            #endregion
            #region malePlanetfallChineseCulture
            list.Add(new CultureTemplate()
            {
                KeyName = "malePlanetfallChineseCulture",
                AgeInYears = new NormalDistribution() { Min = 18f, Max = 68f },
                CasteKey = "male",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "asianHumanAncestor", //
                CommonFirstNames = new[] { "Wei", "Hao", "Dong", "Ming", "Tao", "Peng" },

                CommonLastNames = new[] { "Li", "Wang", "Zhang", "Liu", "Chen", "Yang", "Huang", "Zhao", "Zhou", "Wu", "Xu", "Sun", "Zhu", "Ma", "Hu", "Guo", "Lin" },
            });
            #endregion
            #region malePlanetfallJapaneseCulture
            list.Add(new CultureTemplate()
            {
                KeyName = "malePlanetfallJapaneseCulture",
                AgeInYears = new NormalDistribution() { Min = 18f, Max = 68f },
                CasteKey = "male",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "asianHumanAncestor", //
                CommonFirstNames = new[] { "Hiroto", "Shota", "Ren", "Sota", "Sora", "Yuto", "Yuma", "Eita", "Sho" },

                CommonLastNames = new[] { "Sato", "Tanaka", "Ito", "Saito", "Kato", "Yoshida", "Yamada", "Sasaki", "Inoue", "Kimura", "Hayashi", "Shimizu" },
            });
            #endregion
            #region malePlanetfallHispanicCulture
            list.Add(new CultureTemplate()
            {
                KeyName = "malePlanetfallHispanicCulture",
                AgeInYears = new NormalDistribution() { Min = 18f, Max = 68f },
                CasteKey = "male",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "hispanicHumanAncestor", //
                CommonFirstNames = new[] { "Adrian", "Agustin", "Alejandro", "Alonso", "Alvaro", "Angel", "Antonio", "Benjamin", "Bruno", "Carlos", "Cesar", "Daniel", "David", "Diego", "Francisco", "Gabriel", "Hugo", "Iker", "Javier", "Jeronimo", "Joaquin", "Jose", "Juan", "Luis", "Mario", "Martin", "Mateo", "Matias", "Miguel", "Nicolas", "Pablo", "Pedro", "Ramon", "Rodrigo", "Samuel", "Santiago", "Sebastian", "Tomas", "Vicente"  },
                UncommonFirstNames = new[] { "Thiago" },
                CommonLastNames = new[] { "Rocha", "Villa", "Vargas", "Sastre", "Rana", "Gebara", "Mendoza", "Viteri", "Aritza", "Ferrer", "Roig", "Gonzalez", "Hernandez", "Ramirez", "Rodriguez", "Gutierrez", "Ortiz", "Morales", "Sanchez", "Martinez", "Lopes", "Dias", "Gomez", "Torres", "Flores", "Garcia", "Ruiz", "Velazques", "Jimenez" , "Cruz", "Iglesia"},
                //      UncommonLastNames = new[] { "Beaumont" }
            });
            #endregion
            #region malePlanetfallAfricanCulture
            list.Add(new CultureTemplate()
            {
                KeyName = "malePlanetfallAfricanCulture",
                AgeInYears = new NormalDistribution() { Min = 18f, Max = 68f },
                CasteKey = "male",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "blackHumanAncestor", //
                CommonFirstNames = new[] { "Kofi", "Christian", "John", "Akan", "Will", "Kojo", "Akachi", "Robert", "Joseph", "Jerome", "Chidi", "Marc", "Kojo", "Jacob", "Mack", "Basile", "Bertrand", "David", "Didier", "Jean" , "Leon", "Marcel", "Max", "Nicolas" },

                CommonLastNames = new[] { "Kwena", "Williams", "Thomas", "Oni", "Eze", "Martins", "Okafor", "Savage", "Lawal", "Laurent", "Abiola", "Adebisi", "Layeni", "Taiwo", "Oyekan", "Balewa", "Iwu", "Sekibo", "Madaki", "Akerele", "Igwe", "Ohakim", "Jakande", "Chike", "Yeboah" },

            });
            #endregion
            /////////////FEMALE   ..make sure females have the same total amount of first names to choose from as the males. When the surnames are distributed, might be a good idea to put a fraction unique for each gender, for more variation ? (to avoid kinship/couples)
            #region femalePlanetfallWhiteAngloCulture
            list.Add(new CultureTemplate()
            {
                KeyName = "femalePlanetfallWhiteAngloCulture", //mp I put a few of the names from the male other cultures here as well, to indicate kinship.
                AgeInYears = new NormalDistribution() { Min = 18f, Max = 68f },
                CasteKey = "female",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "whiteHumanAncestor", //
                NoOfPortraitFlavours = 2, // NEW, there are 2 portraits to choose from
                CommonFirstNames = new[] { "Julie", "Dara", "Ramona", "Christie", "Kirsten", "Laura", "Leona", "Paula", "Mary", "Patricia", "Linda", "Barbara", "Elizabeth", "Jennifer", "Maria", "Susan", "Sophia", "Emily", "Isabella", "Madison", "Olivia", "Emma", "Ava", "Hailey", "Abigail", "Kaitlyn", "Mia", "Ruby", "Evie", "Lily", "Charlotte", "Mia", "Ava", "Emily", "Sofia", "Sophie", "Chloe", "Ella", "Isla", "Amelia", "Isabella", "Grace", "Lucy", "Jessica" }, // 
                UncommonFirstNames = new[] { "Alise", "Emmie", "Karin", "Tara", "Vera", "Harper" },
                CommonLastNames = new[] { "Walker", "Brayman", "Nelson", "Nielsen", "Goan", "Pierce", "Late", "Fritt", "Lavin", "Kaspar", "Novak", "Beaumont", "Vasile", "Baudin", "Green", "Beaumont", "Tash", "Tod", "Traviss", "Smith", "Johnson", "Williams", "Brown", "Jones", "Miller", "Davis", "Wilson", "Thomas", "Moore", "Martin", "Jackson", "Thompson", "White", "Lee", "Harris", "Clark", "Lewis", "Robinson" }, //copied from male

            });
            #endregion

            #region femalePlanetfallRussianCulture
            list.Add(new CultureTemplate()
            {
                KeyName = "femalePlanetfallRussianCulture", //
                AgeInYears = new NormalDistribution() { Min = 18f, Max = 68f },
                CasteKey = "female",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "whiteHumanAncestor", //
                NoOfPortraitFlavours = 2, // NEW, there are 2 portraits to choose from
                CommonFirstNames = new[] { "Irina","Anastasia", "Mariya", "Dariya", "Anna", "Polina", "Viktoria", "Yekaterina", "Sofia", "Alexandra", "Irina", "Kira", "Lana", "Lidiya", "Manya", "Ludmila", "Melana", "Nadya", "Natasha", "Oxana", "Sacha", "Sabina", "Sonya", "Tania", "Valentina", "	Yaneta", "Zhanna" }, // 
                /////////////////!!!!!!REMEMBER RUSSIAN LAST NAMES ARE DIFFERENT FOR FEMALES!//////////////////
                CommonLastNames = new[] { "Nadova","Smirnova", "Ivanova", "Kuznetsova", "Popova", "Sokolova", "Lebedeva", "Kozlova", "Novikova", "Morozova", "Petrova", "Volkova", "Zaytseva", "Yazova", "Zaytseva", "Timmerman", "Stasova", "Tikhonova" }, //added -a to the male names!!!!!!!

            });
            #endregion
            #region femalePlanetfallAfricanCulture
            list.Add(new CultureTemplate()
            {
                KeyName = "femalePlanetfallAfricanCulture",
                AgeInYears = new NormalDistribution() { Min = 18f, Max = 68f },
                CasteKey = "female",
                //mp the portrait img comes from racekey, PortraitSkinType="whitePortrait"  combined with the castekey
                RaceKey = "blackHumanAncestor", //
                CommonFirstNames = new[] { "Augustine", "Sarah", "Ama", "Ayo", "Ezewa", "Daraya", "Kisha", "Kenia", "Kabiite", "Malene", "Nadira", "Safika", "Sisi", "Tana", "Wamani", "Zakia",  "Nkechi", "Nuru", "Julia", "Linda", "Eva", "Mara", "Sanda", "Kiri", "Amara", "Adanna", "Liza", "Lana", "Serena", "Zina", "Yana", "Zuwena", "Tekene" },

                CommonLastNames = new[] { "Kwena", "Michel", "Clark", "Laurent", "Williams", "Thomas", "Oni", "Eze", "Martins", "Okafor", "Savage", "Lawal", "Laurent", "Abiola", "Adebisi", "Layeni", "Taiwo", "Oyekan", "Balewa", "Iwu", "Sekibo", "Madaki", "Akerele", "Igwe", "Ohakim", "Jakande", "Chike", "Allard", "Yeboah" }, // most copied from male

            });
            #endregion
            #endregion

            return list;

        }

    }
}
