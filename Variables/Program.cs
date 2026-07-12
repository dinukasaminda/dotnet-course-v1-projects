using System;

namespace MyConsoleApp;

class Student
{
    public int StudentId {get;set;}

    public string Name {get;set;}

    public string? Address {get; set;}

    public Student(int studentId, string name)
    {
        StudentId = studentId;
        Name = name;
    }

    public void SetAddress(string address)
    {

        Address = address;
    }
}
public class Program
{
    public static void Main(string[] args)
    {
        string? name = "Alice" ; 

        Console.WriteLine($"Name: {name}");

        name = null;

        var student1 = new Student(10001, "Alice");
        student1.SetAddress("Colombo");

        const float g = 9.9999f;

        const int x = 5;

        var items = new List<int>{5,2,3,4};
        
        Console.WriteLine($"Number of items: {items.Count}");
        items.Add(23);
        Console.WriteLine($"Number of items: {items.Count}");

        Console.WriteLine("Value at zero: "+ items[0]);


        var myBag = new List<object>{};
        myBag.Add(23);   // index = 0
        myBag.Add("Bob"); // index = 1
        myBag.Add(10.5m); // index = 2

        Console.WriteLine(myBag[0]);

        int age = (int)myBag[0]; // explicit casting
        string myName = (string)myBag[1];
        decimal price = (decimal)myBag[2];

        Console.WriteLine($"Age: {age}");
        Console.WriteLine($"myName: {myName}");
        Console.WriteLine($"price: {price}");


        double distance = 75.9343;
        int intDistance = (int)distance;

        Console.WriteLine($"int Distance: {intDistance}");



    }
}