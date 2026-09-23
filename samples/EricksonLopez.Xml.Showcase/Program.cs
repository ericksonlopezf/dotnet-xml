// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using EricksonLopez.Xml.Showcase.Levels;

namespace EricksonLopez.Xml.Showcase;

/// <summary>
/// Provides the entry point for the executable showcase demonstrating XML validation features.
/// </summary>
public static class Program
{
    /// <summary>
    /// Executes the showcase application asynchronously.
    /// </summary>
    /// <param name="args">The command-line arguments passed to the application</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the process exit code.
    /// </returns>
    public static async Task<int> Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
╔══════════════════════════════════════════════════════════════════════════════════╗
║               ERICKSONLOPEZ.XML.VALIDATION — OFFICIAL SHOWCASE                   ║
║               Reference Implementation & Executable Documentation                ║
╚══════════════════════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        string targetLevel = args.Length > 0 ? args[0].ToLowerInvariant().Trim() : "all";

        var totalStopwatch = Stopwatch.StartNew();

        try
        {
            switch (targetLevel)
            {
                case "0":
                case "level00":
                case "conceptual":
                    await Level00Conceptual.RunAsync();
                    break;

                case "1":
                case "level01":
                case "quickstart":
                    await Level01QuickStart.RunAsync();
                    break;

                case "2":
                case "level02":
                case "configuration":
                    await Level02FullConfiguration.RunAsync();
                    break;

                case "3":
                case "level03":
                case "usecases":
                    await Level03RealWorldUseCases.RunAsync();
                    break;

                case "4":
                case "level04":
                case "integration":
                    await Level04AdvancedIntegration.RunAsync();
                    break;

                case "5":
                case "level05":
                case "concurrency":
                    await Level05ProcessingAndConcurrency.RunAsync();
                    break;

                case "6":
                case "level06":
                case "errors":
                    await Level06ErrorHandlingAndClassification.RunAsync();
                    break;

                case "7":
                case "level07":
                case "scalability":
                    await Level07ScalabilityAndThroughput.RunAsync();
                    break;

                case "8":
                case "level08":
                case "customization":
                    await Level08CustomizationAndExtensibility.RunAsync();
                    break;

                case "9":
                case "level09":
                case "boundaries":
                case "extensions":
                    await Level09ArchitecturalBoundariesAndConsumers.RunAsync();
                    break;

                case "10":
                case "level10":
                case "enterprise":
                    await Level10EnterpriseArchitecture.RunAsync();
                    break;

                case "all":
                default:
                    await RunAllLevelsAsync();
                    break;
            }

            totalStopwatch.Stop();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n================================================================================");
            Console.WriteLine($"  SUMMARY: All requested levels executed successfully.");
            Console.WriteLine($"  Total Showcase execution time: {totalStopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine("================================================================================\n");
            Console.ResetColor();

            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[FATAL] Error in Showcase execution: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
            return 1;
        }
    }

    private static async Task RunAllLevelsAsync()
    {
        await Level00Conceptual.RunAsync();
        await Level01QuickStart.RunAsync();
        await Level02FullConfiguration.RunAsync();
        await Level03RealWorldUseCases.RunAsync();
        await Level04AdvancedIntegration.RunAsync();
        await Level05ProcessingAndConcurrency.RunAsync();
        await Level06ErrorHandlingAndClassification.RunAsync();
        await Level07ScalabilityAndThroughput.RunAsync();
        await Level08CustomizationAndExtensibility.RunAsync();
        await Level09ArchitecturalBoundariesAndConsumers.RunAsync();
        await Level10EnterpriseArchitecture.RunAsync();
    }
}
