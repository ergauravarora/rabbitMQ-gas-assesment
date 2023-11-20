using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System;
using System.Collections.Generic;
using MassTransit;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace ConsoleApp1
{



public class SensorValue
    {
    public DateTimeOffset MeasurementTime { get; set; }
    public string Name { get; set; } = "";
    public string Element { get; set; } = "";
    public double Value { get; set; } = 0;
    public string Status { get; set; } = "";
    public string Unit { get; set; } = "";
    public double Minimum { get; set; } = 0;
    public double Maximum { get; set; } = 0;
    public string Fermenter { get; set; } = "";
    public bool Actor { get; set; }
    public bool License { get; set; }
    public string Serial { get; set; } = "";
}

public class SensorEvent
{
    public SensorValue SensorValue { get; set; }
}


    class Program
    {
        static async Task Main()
        {
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri("rabbitmq://localhost"), h => { });
            });

            await busControl.StartAsync();

            Console.WriteLine("Publisher app started...");

            var random = new Random();

            while (true)
            {
                var sensorValue = new SensorValue
                {
                    MeasurementTime = DateTimeOffset.Now,
                    Name = "Sensor01",
                    Element = "Temperature",
                    Value = random.NextDouble() * 100,
                    Status = "OK",
                    Unit = "unit",
                    Minimum = 0,
                    Maximum = 100,
                    Fermenter = "unused",
                    Actor = false,
                    License = false,
                    Serial = Guid.NewGuid().ToString()
                };

                await busControl.Publish(new SensorEvent { SensorValue = sensorValue });

                Console.WriteLine($"Published: {sensorValue.Name} {sensorValue.Element} {sensorValue.Value}");

                // Sleep for ~5 seconds
                await Task.Delay(5000);
            }
        }
    }


}
