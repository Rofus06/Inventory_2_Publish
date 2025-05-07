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
    private Player player;
    private bool inventoryOpen = false;
    private bool inCombat = false;
    private int day = 1;
    private List<int> zombieDistances;
    private int currentZombieIndex = 0;

    public Game()
    {
        player = new Player(day); // Initialize player with current day
    }

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
        ConsoleKeyInfo key;
        try
        {
            key = Console.ReadKey(true);
        }
        catch (InvalidOperationException)
        {
            // Fallback for environments where ReadKey doesn't work
            Console.WriteLine("Please enter a command (TAB, 1-5, 8, 9, 0):");
            string input = Console.ReadLine();
            if (string.IsNullOrEmpty(input)) return;
            
            // Convert first character to ConsoleKeyInfo
            char firstChar = input.ToUpper()[0];
            key = new ConsoleKeyInfo(firstChar, (ConsoleKey)firstChar, false, false, false);
        }

        if (key.Key == ConsoleKey.Tab)
        {
            ToggleInventory();
        }
        else if (key.Key >= ConsoleKey.D1 && key.Key <= ConsoleKey.D5)
        {
            int slot = key.Key - ConsoleKey.D1;
            if (slot + 1 <= day) // Only allow equipping unlocked items
            {
                player.EquipItem(slot);
                if (inCombat)
                {
                    Console.WriteLine($"You aim your {player.EquippedWeapon.Name} at the zombie...");
                }
            }
            else
            {
                Console.WriteLine("This item is locked! Complete more days to unlock it.");
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
        Console.Write("\nType a command (shoot, wait, 1-5 to switch Main weapon): ");
        string input;
        try
        {
            input = Console.ReadLine()?.ToLower() ?? string.Empty;
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("Error reading input. Please try again.");
            return;
        }

        if (input.Length == 1 && input[0] >= '1' && input[0] <= '5')
        {
            int slot = input[0] - '1';
            if (slot + 1 <= day) // Bara tillåt tillgång till upplåsta vapen
            {
                player.EquipItem(slot);
            }
            else
            {
                Console.WriteLine("This item is locked! Complete more days to unlock it.");
            }
            return; // Hoppa över resten
        }

        if (input == "shoot")
        {
            if (player.EquippedWeapon == null)
            {
                Console.WriteLine("Press 1-5 to equip a weapon first!");
                return;
            }

            // Find the closest zombie in range (like 2nd zombie 20 meter evey other zombie more then 20 it will go for the closest zombie)
            int closestZombieIndex = -1;
            int closestDistance = int.MaxValue;
            
            for (int i = 0; i < zombieDistances.Count; i++)
            {
                if (zombieDistances[i] <= 0)
                {
                    Console.WriteLine("A zombie reached you! You died!");
                    EndCombat(false);
                    return;
                }

                if (zombieDistances[i] <= player.EquippedWeapon.Range && 
                    zombieDistances[i] < closestDistance)
                {
                    closestZombieIndex = i;
                    closestDistance = zombieDistances[i];
                }
            }

            if (closestZombieIndex >= 0)
            {
                // When you kill zombie...
                Console.WriteLine($"You killed zombie {closestZombieIndex + 1} with your {player.EquippedWeapon.Name}!");
                zombieDistances[closestZombieIndex] = int.MaxValue;

                // Move remaining zombies forward (except dead ones)
                Console.WriteLine("The remaining zombies move forward!");
                for (int i = 0; i < zombieDistances.Count; i++)
                {
                    if (zombieDistances[i] != int.MaxValue) // If not dead
                    {
                        zombieDistances[i] -= 5;
                        if (zombieDistances[i] <= 0)
                        {
                            Console.WriteLine($"Zombie {i + 1} reached you! You died!");
                            EndCombat(false);
                            return;
                        }
                        Console.WriteLine($"Zombie {i + 1} is now {zombieDistances[i]} meters away.");
                    }
                }

                // Check if all zombies are dead
                if (zombieDistances.TrueForAll(d => d == int.MaxValue))
                {
                    EndCombat(true);
                }
            }
            else
            {
                Console.WriteLine("No zombies are in range!");
            }
        }
        else if (input == "wait")
        {
            Console.WriteLine("You wait cautiously...");
            // All zombies move closer
            for (int i = 0; i < zombieDistances.Count; i++)
            {
                if (zombieDistances[i] != int.MaxValue) // If not dead
                {
                    zombieDistances[i] -= 5;
                    if (zombieDistances[i] <= 0)
                    {
                        Console.WriteLine($"Zombie {i + 1} reached you! You died!");
                        EndCombat(false);
                        return;
                    }
                    Console.WriteLine($"Zombie {i + 1} is now {zombieDistances[i]} meters away.");
                }
            }
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
        for (int i = 0; i < zombieDistances.Count; i++)
        {
            Console.WriteLine($"Zombie {i + 1} is {zombieDistances[i]} meters away.");
        }
    }

    private void EndCombat(bool victory)
    {
        inCombat = false;
        if (victory)
        {
            Console.WriteLine("\nYou survived the day!");
            Console.ReadLine();
            day++;
            player = new Player(day); // Update player with new day
            ShowMenu();
        }
        else
        {
            Console.WriteLine("\nGame Over!");
            Console.ReadLine();
            Environment.Exit(0);
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
        Console.WriteLine("Press TAB to open inventory. MUST DO IN THE BEGINNING OF EVERY DAY!");
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
        if (day < 2)
        {
            Console.WriteLine("You need to complete at least day 1 to unlock item swapping!");
            return;
        }

        Console.Write("Enter the two item slots to swap (e.g., '1 2'): ");
        string[] input = Console.ReadLine().Split();

        if (input.Length == 2 && int.TryParse(input[0], out int slot1) && int.TryParse(input[1], out int slot2))
        {
            // Check if both slots are unlocked
            if (slot1 > day || slot2 > day)
            {
                Console.WriteLine("You can't swap with locked slots!");
                return;
            }

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
        int zombieCount = Math.Min(day + 1, 5); // Max 5 zombies
        zombieDistances = new List<int>();
        Random rnd = new Random();
        
        for (int i = 0; i < zombieCount; i++)
        {
            zombieDistances.Add(rnd.Next(10, 30 + (day * 10))); // Zombies start further away as days progress
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
    public Inventory inventory;
    public Weapon EquippedWeapon { get; private set; }

    public Player(int day)
    {
        inventory = new Inventory(day);
    }

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
    private int day;

    public Inventory(int currentDay)
    {
        day = currentDay;
        items = new List<Weapon>();
    }

    public async Task DrawRectangles(int currentDay)
    {
        day = currentDay;
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

    public Weapon GetItem(int index)
    {
        if (index >= day) return null; // Can't get locked items
        return (index >= 0 && index < items.Count) ? items[index] : null;
    }

    public Weapon GetItemByName(string name) => items.Find(item => item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    
    public void SwapItems(int index1, int index2)
    {
        if (index1 >= day || index2 >= day) return; // Can't swap with locked items
        if (index1 >= 0 && index1 < items.Count && index2 >= 0 && index2 < items.Count)
        {
            (items[index1], items[index2]) = (items[index2], items[index1]);
        }
    }
}

public class GameItem
{
    public string Name { get; set; }

    public GameItem(string name)
    {
        Name = name;
    }

    public virtual void DisplayInfo()
    {
        Console.WriteLine($"Item: {Name}");
    }
}

public class Weapon : GameItem
{
    public int Range { get; set; }

    public Weapon(string name, int range) : base(name)
    {
        Range = range;
    }

    public override void DisplayInfo()
    {
        Console.WriteLine($"Weapon: {Name}, Range: {Range}");
    }

    public override string ToString()
    {
        return $"{Name} (Range: {Range})";
    }
}