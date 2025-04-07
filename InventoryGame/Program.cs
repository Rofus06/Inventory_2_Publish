using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static void Main()
    {
        Game game = new Game();
        game.Run();
    }
}

class Game
{
    private Player player = new Player();
    private bool inventoryOpen = false;
    private bool inCombat = false;
    private int day = 1;
    private List<int> zombieDistances;
    private int currentZombieIndex = 0;

    public void Run()
    {
        ShowMenu();

        while (true)
        {
            if (inCombat)
            {
                HandleCombatInput();
            }
            else
            {
                HandleGameInput();
            }
        }
    }

    private void HandleGameInput()
    {
        ConsoleKeyInfo key = Console.ReadKey(true);

        if (key.Key == ConsoleKey.Tab)
        {
            ToggleInventory();
        }
        else if (key.Key >= ConsoleKey.D1 && key.Key <= ConsoleKey.D5)
        {
            player.EquipItem(key.Key - ConsoleKey.D1);
            if (inCombat)
            {
                Console.WriteLine($"You aim your {player.EquippedWeapon.Name} at the zombie...");
            }
        }
        else if (key.Key == ConsoleKey.D8)
        {
            DisplayWeaponInfo();
        }
        else if (key.Key == ConsoleKey.D9)
        {
            SwapItems();
        }
        else if (key.Key == ConsoleKey.D0)
        {
            StartDay();
        }
    }

    private void HandleCombatInput()
    {
        Console.Write("\nType a command (shoot, wait): ");
        string input = Console.ReadLine().ToLower();

        if (input == "shoot")
        {
            if (player.EquippedWeapon == null)
            {
                Console.WriteLine("You have no weapon equipped!");
                return;
            }

            int zombieDistance = zombieDistances[currentZombieIndex];
            if (zombieDistance <= player.EquippedWeapon.Range)
            {
                Console.WriteLine($"You killed the zombie with your {player.EquippedWeapon.Name}!");
                currentZombieIndex++;
                
                if (currentZombieIndex >= zombieDistances.Count)
                {
                    EndCombat(true);
                }
            }
            else
            {
                Console.WriteLine($"The zombie is too far away! (Your {player.EquippedWeapon.Name} range: {player.EquippedWeapon.Range}m, Zombie distance: {zombieDistance}m)");
            }
        }
        else if (input == "wait")
        {
            Console.WriteLine("You wait cautiously...");
            // Zombie moves closer
            zombieDistances[currentZombieIndex] -= 5;
            Console.WriteLine($"The zombie is now {zombieDistances[currentZombieIndex]} meters away.");
        }
        else
        {
            Console.WriteLine("Invalid command.");
        }
    }

    private void StartCombat()
    {
        inCombat = true;
        currentZombieIndex = 0;
        Console.WriteLine("\nCombat has started! Type 'shoot' or 'wait'.");
        Console.WriteLine($"First zombie is {zombieDistances[0]} meters away.");
    }

    private void EndCombat(bool victory)
    {
        inCombat = false;
        if (victory)
        {
            Console.WriteLine("\nYou survived the day!");
            day++;
            ShowMenu();
        }
    }

    private void ShowMenu()
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            Console.WriteLine("\n\n[Warning: Could not clear console. Continuing anyway...]\n");
        }

        Console.WriteLine($"Welcome to the game! (Day {day})");
        Console.WriteLine("Press TAB to open inventory.");
        Console.WriteLine("Press 1-5 to equip an item.");
        Console.WriteLine("Press 8 for more info.");
        Console.WriteLine("Press 9 to swap items.");
        Console.WriteLine("Press 0 to start the day.");
    }

    private void ToggleInventory()
    {
        if (inventoryOpen)
        {
            inventoryOpen = false;
            ShowMenu();
        }
        else
        {
            inventoryOpen = true;
            player.ShowInventory(day);
        }
    }

    private void DisplayWeaponInfo()
    {
        Console.Write("Enter the name of the item for more information: ");
        string itemName = Console.ReadLine();

        Weapon item = player.GetInventoryItemByName(itemName);
        if (item != null)
        {
            Console.WriteLine($"Item: {item.Name}, Range: {item.Range}m");
        }
        else
        {
            Console.WriteLine("Item not found.");
        }
    }

    private void SwapItems()
    {
        Console.Write("Enter the two item slots to swap (e.g., '1 2'): ");
        string[] input = Console.ReadLine().Split();

        if (input.Length == 2 && int.TryParse(input[0], out int slot1) && int.TryParse(input[1], out int slot2))
        {
            player.SwapInventoryItems(slot1 - 1, slot2 - 1);
            Console.WriteLine("Items swapped!");
        }
        else
        {
            Console.WriteLine("Invalid input!");
        }
    }

    private void StartDay()
    {
        Console.Clear();
        Console.WriteLine($"A new day has begun! (Day {day})");
        Thread.Sleep(1000);

        // Simple zombie generation - more zombies as days progress
        int zombieCount = Math.Min(day, 5); // Max 5 zombies
        zombieDistances = new List<int>();
        Random rnd = new Random();
        
        for (int i = 0; i < zombieCount; i++)
        {
            zombieDistances.Add(rnd.Next(10, 100));
        }

        Console.WriteLine($"There are {zombieDistances.Count} zombies approaching!");

        for (int i = 0; i < zombieDistances.Count; i++)
        {
            Console.WriteLine($"Zombie {i + 1} is {zombieDistances[i]} meters away");
            Thread.Sleep(500);
        }

        StartCombat();
    }
}

class Player
{
    public Inventory inventory = new Inventory();
    public Weapon EquippedWeapon { get; private set; }

    public async void ShowInventory(int day)
    {
        await inventory.DrawRectangles(day);
    }

    public void EquipItem(int index)
    {
        Weapon item = inventory.GetItem(index);
        if (item != null)
        {
            EquippedWeapon = item;
            Console.WriteLine($"You equipped {item.Name}.");
        }
        else
        {
            Console.WriteLine("No item in this slot.");
        }
    }

    public Weapon GetInventoryItemByName(string name)
    {
        return inventory.GetItemByName(name);
    }

    public void SwapInventoryItems(int index1, int index2)
    {
        inventory.SwapItems(index1, index2);
    }
}

class Inventory
{
    private List<Weapon> items;

    public Inventory()
    {
        // Initialize with empty inventory
        items = new List<Weapon>();
    }

    public async Task DrawRectangles(int day)
    {
        // Get weapons from API
        items = await API_Inventory.GetWeaponsAsync();
        
        Console.Clear();
        Console.WriteLine("Inventory:");

        string[] itemNames = new string[8];

        for (int i = 0; i < 8; i++)
        {
            itemNames[i] = (i < day) ? items[i].Name : "Locked";
        }

        string[] rectangle =
        {
            "+-----------+  +-----------+  +-----------+  +-----------+  +-----------+",
            "|           |  |           |  |           |  |           |  |           |",
            "|           |  |           |  |           |  |           |  |           |",
            "|  {0,-9}|  |  {1,-9}|  |  {2,-9}|  |  {3,-9}|  |  {4,-9}|",
            "|           |  |           |  |           |  |           |  |           |",
            "|           |  |           |  |           |  |           |  |           |",
            "|__________1|  |__________2|  |__________3|  |__________4|  |__________5|",
            "+-----------+  +-----------+  +-----------+  +-----------+  +-----------+",
            "|           |  |           |  |           |  |           |  |           |",
            "|           |  |           |  |           |  |           |  |           |",
            "|  {5,-9}|  |  {6,-9}|  |  {7,-9}|  |           |  |           |",
            "|           |  |           |  |           |  |           |  |           |",
            "|           |  |           |  |           |  |           |  |           |",
            "|__________6|  |__________7|  |__________8|  |___________|  |___________|"
        };

        Console.WriteLine(string.Format(string.Join("\n", rectangle), itemNames));
        Console.WriteLine("\nPress TAB to close inventory.");
    }

    public Weapon GetItem(int index) => (index >= 0 && index < items.Count) ? items[index] : null;
    public Weapon GetItemByName(string name) => items.Find(item => item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    public void SwapItems(int index1, int index2) => (items[index1], items[index2]) = (items[index2], items[index1]);
}

public class Weapon
{
    public string Name { get; }
    public int Range { get; }

    public Weapon(string name, int range)
    {
        Name = name;
        Range = range;
    }
}