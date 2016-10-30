using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PlaceableObject))]
public class CallsignManager : MonoBehaviour
{
    private int m_maxAttempts = 100;


    void Awake()
    {
        if (m_usedIndices == null)
            ResetUsedIndices();

        if (name.EndsWith("(Clone)"))
            name = name.Remove(name.Length - 7);

        var placeableObject = GetComponent<PlaceableObject>();

        var placeableObjectAir = placeableObject as PlaceableObjectAir;

        int index = -1;
        
        if (placeableObjectAir != null)
        {
            int attempt = 1;
            bool indexFound = false;
            while (!indexFound && attempt <= m_maxAttempts)
            {
                index = Random.Range(0, m_callsigns.Count);

                if (!m_usedIndices.Contains(index))
                {
                    indexFound = true;
                    m_usedIndices.Add(index);
                }

                attempt++;
            }
        }

        if (index >= 0)
            name = string.Format("{0}: {1}", name, m_callsigns[index]);
    }


    public static void ResetUsedIndices()
    {
        m_usedIndices = new HashSet<int>();
    }


    private static HashSet<int> m_usedIndices;


    private static List<string> m_callsigns = new List<string>
    {
        "Viper",
        "Stinger",
        "Wolfman",
        "Merlin",
        "Goose",
        "Iceman",
        "Chipper",
        "Maverick",
        "Jester",
        "Sundown",
        "Slider",
        "Cougar",
        "Rogue",
        "Red",
        "Yellow",
        "Blue",
        "Acklay",
        "Acrobat",
        "Angel",
        "Azure",
        "Babs",
        "Backhoe",
        "Bandit",
        "Barefoot",
        "Battery",
        "Bigtop",
        "Birdseye",
        "Bit",
        "Bogie",
        "Bones",
        "Bonnie",
        "Boxer",
        "Burn",
        "Buckethead",
        "Cameo",
        "Candy",
        "Carbon",
        "Cavalier",
        "Chip",
        "Citadel",
        "Citrus",
        "Cobweb",
        "Cowboy",
        "Crank",
        "Crisp",
        "Critter",
        "Crunch",
        "Cutter",
        "Cycle",
        "Cypher",
        "Dancer",
        "Deckhand",
        "Dekk",
        "Derby",
        "Digger",
        "Dog",
        "Dragon",
        "Drake",
        "Drez",
        "Dynasty",
        "Eagle",
        "Evergreen",
        "Endar",
        "Fable",
        "Fade",
        "Feral",
        "Fox",
        "Frostbite",
        "Gatecrash",
        "Geezer",
        "Glimmer",
        "Gunz",
        "Gutter",
        "Hawk",
        "Hitch",
        "Hoop",
        "Hound",
        "Huck",
        "Hudson",
        "Honey",
        "Indigo",
        "Iron",
        "Jazz",
        "Jersey",
        "Knave",
        "Lightfoot",
        "Lotus",
        "Lucky",
        "Lunatic",
        "Machete",
        "Meteor",
        "Mismatch",
        "Mouse",
        "Mustang",
        "Orchid",
        "Pax",
        "Peacemaker",
        "Porkchop",
        "Rabbit",
        "Radiant",
        "Razor",
        "Ruckus",
        "Scrap",
        "Shark",
        "Seven",
        "Shade",
        "Shadow",
        "Shamrock",
        "Shark",
        "Slick",
        "Snowbank",
        "Stutter",
        "Sugar",
        "Sunburn",
        "Tiller",
        "Tink",
        "Tranquil",
        "Unicorn",
        "Vixen",
        "Volcano",
        "Wild",
        "Wraith",
        "Zero",
        "Trinity",
        "Cipher",
        "Falcon",
        "Crow",
        "Helo",
        "Showboat",
        "Athena",
        "Hot Dog",
        "Redwing",
        "Hardball",
        "Fuzzy",
        "Bingo",
        "Hex",
        "Hotshot",
        "Terra",
        "Wally",
        "Freaker",
        "Red Devil",
        "Feline",
        "Hiccup",
        "Snitch",
        "Chopper",
        "Snicker",
        "Tailgate",
        "Blindspot",
        "Bomber",
        "Butch",
        "Catbird",
        "Digger",
        "Dune",
        "Famous",
        "Hyper",
        "Kingston",
        "Ninja",
        "Rash",
        "Rocket",
        "Ruins",
        "Sniper",
        "Stinger",
        "Snaps",
        "Thumper",
        "Husker",
        "Apollo",
        "Bulldog",
        "Shooter",
        "Dipper",
        "Starbuck",
        "Jolly",
        "Karma",
        "Buster",
        "Racetrack",
        "Skulls",
        "Crashdown",
        "Flat Top",
        "Boomer",
        "Longshot",
        "Chuckles",
        "Flyboy",
        "Fireball",
        "Sandman",
        "Sheppard",
        "Dash",
        "Headcase",
        "Swordsman",
        "Anvil",
        "Chinstrap",
        "Gumball",
        "Polo",
        "Casey",
        "Snake",
        "Crash",
        "Bubba",
        "Flash",
        "Nuke",
        "Rebel",
        "Sunny",
        "Rock Star",
        "Whiplash",
        "Thumper",
        "Honey",
        "Frosty",
        "Bash",
        "Hooper",
        "Jackson",
        "Spinner",
        "Buzzer",
        "Beehive",
        "Joker,",
        "Wedge,",
        "Winger,",
        "Bones,",
        "Beetle,",
        "Sleeper",
        "Doom,",
        "Lefty",
        "Silent",
        "Quiet"
    };



    
}
