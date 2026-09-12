using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities.Biological;

namespace UWGame.SimSide.AllGameData
{
    public class OrdersLoader
    {
        public static List<BioOrderType> Init()
        {
            List<BioOrderType> list = new List<BioOrderType>();

                list.Add(new BioOrderType()
                {
                    KeyName = "quaditeOrder",
                    Name = "Quadites",
                    Description = "\n FEEDING CLASSIFICATION: Memebers include herbivores, predators and scavengers\n \n HEIGHT: Up to 1 m\n \n ANATOMY\n Order of animals widespread on this continent. Name hints at their 4-corner shape.\n Mouth opening is situated on the bottom of the body, the digestive canals extending radially from the center. On the top are bioluminescent communication organs as well as the sensory receptors.\n Covered in a strong exoskeleton.\n \n BEHAVIOR\n Most quadites are social animals, living in smaller packs or large colonies. The most dangerous quadite we have encountered, the Swarm quadite, had several thousand individuals forming a highly ferocious swarm.\n Quadites emerge from burrows, caves and crevices to hunt the plains. They prefer dry, firm soil and are rarely seen in swamps or marshes.\n \n SENSES\n We believe they locate food through vision and smell. Senses are highly developed, especially the eyes, which are able to perceive multispectral images as well as polarized light."
                    //specific for twinklers:Usually, all four legs are capable of stabbing and injecting venom. Venom strength varies but is often lethal........ , an ability which probably aids the animal with hitting its prey.\n Infrared vision  allows them to hunt very efficiently at night while maintaining constant communication.\n \n THREAT FACTOR\n Medium to High. Humans are sometimes seen as prey and the speed and numbers of quadites make them able to overrun us. Their hard shell and sharp talons present a demanding challenge in close combat and it is therefore recommended that they be dispatched from a distance with ranged weapons ......\n \n COMPETITORS\n Compete with other species of quadites and often patricians.                  
                });

                list.Add(new BioOrderType()
                {
                    KeyName = "bushDragonOrder",
                    Name = "Bush dragons",
                    Description = "\n FEEDING CLASSIFICATION: Omnivore. Eats low vegetation, small animals, carrion.\n \n HEIGHT: Up to 1.5 m (wings excluded)\n \n ANATOMY\n Waddling, 3-legged animal which has developed wings, not for flight but for display purposes. Likely used to dissuade predators and possibly in mating behaviour. The creature has a defensive weapon in the form of a chemical spray.\n \n BEHAVIOR\n If approached, bush dragons will defend themselves much in the manner of the terran skunk. Against the quadites the spray seems to be particularly effective, causing incapacitation and even death.\n \n THREAT LEVEL\n Medium. The slow-moving creatures protect their herd, but if distance is observed they do not attack. The toxicity to humans of their defensive chemical is yet to be determined, but caution is advised.",
                               

                });

                list.Add(new BioOrderType()
                {
                    KeyName = "binalRatOrder",
                    Name = "Binal rats",
                    Description = "\n FEEDING CLASSIFICATION: Its flexible diet means it can survive almost anywhere.\n \n HEIGHT: 10 to 25 cm\n \n ANATOMY\n This order shares some characteristics with the thunder chickens. A common feature is retractable tentacles around the mouth used for feeding. They also have in common an antenna on top of the head for detecting dangers.\n \n BEHAVIOR\n Pervasive, very adaptive order of animals.\n \n THREAT LEVEL\n They are fearful of humans and their presence normally does not cause any conflicts as they will stay away from us.\n \n ENEMIES\n Preyed on by quadites.\n \n NOTES\n Not worth hunting since they are neither a pest or a source of food (their habit of eating plants that are poisonous to humans makes their flesh inedible to us.)",


                });

                list.Add(new BioOrderType()
                {
                    KeyName = "thunderChickenOrder",
                    Name = "Thunder chickens",
                    Description = "\n FEEDING CLASSIFICATION: Omnivore. Eats primarily nuts, mushrooms, bugs\n \n HEIGHT: 30 to 70 cm\n \n ANATOMY\n A common feature of this order is retractable tentacles around the mouth used for feeding. They also have in common an antenna on top of the head for detecting dangers.\n \n BEHAVIOR\n  When threatened they will be quick to flee, sometimes emitting a loud sound.\n \n THREAT LEVEL\n None.\n \n ENEMIES\n Depending on habitat, they are preyed on by different species of quadites.\n \n NOTES\n Most are fit to consume by humans, offering a good source of protein and a pleasant taste.",


                });

                list.Add(new BioOrderType()
                {
                    KeyName = "forestGuardianOrder",
                    Name = "Forest guardians",
                    Description = "\n N/A",
                    
                });

                list.Add(new BioOrderType()
                {
                    KeyName = "patricianOrder",
                    Name = "Patricians",
                    Description = "\n FEEDING CLASSIFICATION: Carnivore. Eats fish, slugs, smaller animals\n \n HEIGHT: Up to 3 m\n \n ANATOMY\n Vertical, octahedron-shaped body protected by exoskeleton. Its four legs are arranged symmetrically as are the sensory organs on the top.\n Its regal posture, 'crown' and territorial behaviour made researchers name the animal for the ancient Roman land holders.\n \n BEHAVIOR\n Highly territorial animal which jealously protects its hunting grounds. Feeds on fish that live within the muckroot water canals. Kills by stabbing with a venomous spear that extends from its legs. With the spear, digestive enzymes are then pumped into the prey and the liquified tissues are sucked out.\n \n THREAT LEVEL\n Medium. The animal will attack if its territory is entered, but moves rather slowly.",

                });

                list.Add(new BioOrderType()
                {
                    KeyName = "demonTreeOrder",
                    Name = "Dendronts", //Dendropods not used
                    Description = "N/A",

                });

                list.Add(new BioOrderType()
                {
                    KeyName = "carnufexOrder",
                    Name = "Carnufexes",
                    Description = "N/A",

                });


                list.Add(new BioOrderType()
                {
                    KeyName = "spikePlantOrder",
                    Name = "Ursinix",
                    Description = "N/A",

                });

                list.Add(new BioOrderType()
                {
                    KeyName = "wormOrder",
                    Name = "Megapods",
                    Description = "N/A",

                });

                list.Add(new BioOrderType()
                {
                    KeyName = "carnivoraOrder",
                    Name = "Carnivora",
                    Description = "Diverse order of Earth animals that includes over 280 species of mammals. Members include weasel, tiger, bear and wolf and primarily eat meat.",

                });


                return list;


        }


    }
}
