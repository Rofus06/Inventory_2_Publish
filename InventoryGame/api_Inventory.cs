using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class API_Inventory
{
    private const string ApiUrl = "https://67f37807ec56ec1a36d5eb8b.mockapi.io/Inventory/WeaponName";
    private static readonly HttpClient client = new HttpClient();
    private static List<Weapon> cachedWeapons;
    

    public static async Task<List<Weapon>> GetWeaponsAsync()
    {
        if (cachedWeapons != null) return cachedWeapons;

        try
        {
            var response = await client.GetStringAsync(ApiUrl);
            var apiWeapons = JsonConvert.DeserializeObject<List<ApiWeapon>>(response);

            cachedWeapons = new List<Weapon>();
            foreach (var apiWeapon in apiWeapons)
            {
                // Map API weapons to game weapons with their properties
                var weapon = CreateWeaponFromApi(apiWeapon);
                if (weapon != null)
                {
                    cachedWeapons.Add(weapon);
                }
            }

            return cachedWeapons;
        }
        catch (Exception)
        {
            // Fallback if API fails
            return GetDefaultWeapons();
        }
    }

    private static Weapon? CreateWeaponFromApi(ApiWeapon apiWeapon)
    {
        // Map API weapons to game weapons with appropriate properties
        return apiWeapon.id switch
        {
            "1" => new Weapon("Pistol", 50),
            "2" => new Weapon("Knife", 5),
            "3" => new Weapon("Shotgun", 20),
            "4" => new Weapon("Rifle", 100),
            "5" => new Weapon("Sniper", 200),
            "6" => new Weapon("Grenade", 15),
            "7" => new Weapon("Flashbang", 10),
            "8" => new Weapon("Smoke", 0),
            _ => null
        };
    }

    private static List<Weapon> GetDefaultWeapons()
    {
        // Default weapons if API is unavailable
        return new List<Weapon>
        {
            new Weapon("Pistol", 50),
            new Weapon("Knife", 5),
            new Weapon("Shotgun", 20),
            new Weapon("Rifle", 100),
            new Weapon("Sniper", 200),
            new Weapon("Grenade", 15),
            new Weapon("Flashbang", 10),
            new Weapon("Smoke", 0)
        };
    }

    private class ApiWeapon
    {
        public string name { get; set; }
        public string id { get; set; }
    }
}