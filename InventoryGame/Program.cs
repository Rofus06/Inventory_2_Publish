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

    public void Run() //did not wanna start so i needed to chance the code took me so fucking long!
    {
        ShowMenu();

        while (true)
        {
            ConsoleKeyInfo key;
            
            try
            {
                key = Console.ReadKey(true); // Tries to read key
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("No console available for key input. Press Enter to continue...");
                Console.ReadLine(); // Fallback for redirected input
                continue;
            }

            if (!inCombat)
            {
                if (key.Key == ConsoleKey.Tab)
                {
                    ToggleInventory();
                }
                else if (key.Key >= ConsoleKey.D1 && key.Key <= ConsoleKey.D5)
                {
                    player.EquipItem(key.Key - ConsoleKey.D1);
                }
                else if (key.Key == ConsoleKey.D8)
                {
                    Display();
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
            else
            {
                if (key.Key >= ConsoleKey.D1 && key.Key <= ConsoleKey.D5)
                {
                    player.EquipItem(key.Key - ConsoleKey.D1);
                }
            }
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
            Console.WriteLine("\n\n"); // Fallback in case clearing fails
        }
        Console.WriteLine("Welcome to the game!");
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
            player.ShowInventory();
        }
    }

    private void Display()
    {
        Console.WriteLine("Enter the name of the item for more information:");
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
        Console.WriteLine("Enter the two item slots to swap (e.g., '1 2'): ");
        string[] input = Console.ReadLine().Split();

        if (input.Length == 2 && int.TryParse(input[0], out int slot1) && int.TryParse(input[1], out int slot2))
        {
            player.SwapInventoryItems(slot1 - 1, slot2 - 1);
        }
        else
        {
            Console.WriteLine("Invalid input!");
        }
    }

    private void StartDay()
    {
        Console.Clear();
        Console.WriteLine("A new day has begun!");
        Thread.Sleep(1000);
        Console.WriteLine("There are 2 zombies");
        Thread.Sleep(1000);
        Console.WriteLine("1st zombie is 30 meters away");
        Thread.Sleep(1000);
        Console.WriteLine("2nd zombie is 50 meters away");
        Thread.Sleep(100);
        Console.WriteLine("Press 1-5 to equip an item.");

        inCombat = true;
    }
}

class Player
{
    public Inventory inventory = new Inventory();

    public void ShowInventory()
    {
        inventory.DrawRectangles();
    }

    public void EquipItem(int index)
    {
        Weapon item = inventory.GetItem(index);
        if (item != null)
        {
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
    private List<Weapon> items = new List<Weapon> //det här är hur jag tänker vapen systemet ska se ut, vad man får varje dag
    {
        new Weapon("Pistol", 50), //DAY 1 shoot 12 shots
        new Weapon("Knife", 5), // DAY 2 insta kill from 5 meters away
        new Weapon("Shotgun", 20), //DAY 3 yeah it is a shotgun... 20 meters do the least amount of damage etc...
        new Weapon("Rifle", 100), //DAY 4 shoot 30 shots
        new Weapon("Sniper", 200), //DAY 5 can only shoot one shot but insta kill
        new Weapon("Grenade", 15), //DAY 6 explode everything that is 10 meters away
        new Weapon("Flashbang", 10), //DAY 7 makes it so you will stop moving for one round
        new Weapon("Smoke", 0) //DAY 8 makes zombies move 2x slower
    };

    public void DrawRectangles()
    {
        Console.Clear();
        Console.WriteLine("Inventory:");

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
            "|__________6|  |__________7|  |__________8|  |___________|  |___________|",

            "+-----------+  +-----------+  +-----------+  +-----------+  +-----------+",
            "|           |  |           |  |           |  |           |  |           |",
            "|           |  |           |  |           |  |           |  |           |",
            "|           |  |           |  |           |  |           |  |           |",
            "|           |  |           |  |           |  |           |  |           |",
            "|           |  |           |  |           |  |           |  |           |",
            "|___________|  |___________|  |___________|  |___________|  |___________|"
        };

        // Fyll inventory med föremål
        string[] itemNames = new string[8];
        for (int i = 0; i < 8; i++)
        {
            itemNames[i] = i < items.Count ? items[i].Name : "";
        }
        Console.WriteLine("Press TAB to close inventory.");
    }

    public Weapon GetItem(int index)
    {
        return (index >= 0 && index < items.Count) ? items[index] : null;
    }

    public Weapon GetItemByName(string name)
    {
        return items.Find(item => item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public void SwapItems(int index1, int index2)
    {
        if (index1 >= 0 && index1 < items.Count && index2 >= 0 && index2 < items.Count)
        {
            Weapon temp = items[index1];
            items[index1] = items[index2];
            items[index2] = temp;
            Console.WriteLine($"Swapped {items[index1].Name} with {items[index2].Name}.");
        }
        else
        {
            Console.WriteLine("Invalid item positions!");
        }
    }
}

class Weapon
{
    public string Name { get; }
    public int Range { get; }

    public Weapon(string name, int range)
    {
        Name = name;
        Range = range;
    }
}