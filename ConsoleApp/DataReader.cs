using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApp
{
    public class DataReader
    {
        IEnumerable<ImportedObject> ImportedObjects;
        StreamReader streamReader;

        public void ImportAndPrintData(string fileToImport, bool printData = true)
        {
            ImportedObjects = new List<ImportedObject>() { new ImportedObject() };
            if (File.Exists(fileToImport))
            {
                streamReader = new StreamReader(fileToImport);
            }
            else
            {
                string[] lines = { "Type;Name;Schema;ParentName;ParentType;DataType;IsNullable",
                    "Example1;Example2;Example3;Example4;Example5;Example6;NULLABLE",
                    "ExampleA;ExampleB;ExampleC;ExampleD;ExampleE;ExampleF;Not Nullable" };

                using (StreamWriter outputFile = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "data.csv")))
                {
                    foreach (string line in lines)
                        outputFile.WriteLine(line);
                }
                streamReader = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), "dataa.csv"));
            }

            var importedLines = new List<string>();
            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                importedLines.Add(line);
            }

            for (int i = 0; i < importedLines.Count; i++)
            {
                var importedLine = importedLines[i];
                var values = importedLine.Split(';');
                var importedObject = new ImportedObject();
                if (values.Length >= 7)
                {
                    importedObject.Type = values[0];
                    importedObject.Name = values[1];
                    importedObject.Schema = values[2];
                    importedObject.ParentName = values[3];
                    importedObject.ParentType = values[4];
                    importedObject.DataType = values[5];
                    importedObject.IsNullable = values[6];
                    if (AreAllPropertiesNotNull(importedObject))
                    {
                        ((List<ImportedObject>)ImportedObjects).Add(importedObject);
                    }
                }
                else
                {
                    Console.WriteLine($"Line {i + 1} is not in the correct format. Skipping...");
                }
            }

            // clear and correct imported data
            foreach (var importedObject in ImportedObjects)
            {
                if (importedObject.Type != null)
                {
                    importedObject.Type = importedObject.Type.Trim().Replace(" ", "").Replace(Environment.NewLine, "").ToUpper();
                }
                if (importedObject.Name != null)
                {
                    importedObject.Name = importedObject.Name.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                }
                if (importedObject.Schema != null)
                {
                    importedObject.Schema = importedObject.Schema.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                }
                if (importedObject.ParentName != null)
                {
                    importedObject.ParentName = importedObject.ParentName.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                }
                if (importedObject.ParentType != null)
                {
                    importedObject.ParentType = importedObject.ParentType.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                }
            }

            // assign number of children
            for (int i = 0; i < ImportedObjects.Count(); i++)
            {
                var importedObject = ImportedObjects.ToArray()[i];
                foreach (var impObj in ImportedObjects)
                {
                    if (impObj.ParentType == importedObject.Type)
                    {
                        if (impObj.ParentName == importedObject.Name)
                        {
                            importedObject.NumberOfChildren = 1 + importedObject.NumberOfChildren;
                        }
                    }
                }
            }

            foreach (var database in ImportedObjects)
            {
                if (database.Type == "DATABASE")
                {
                    Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");

                    // print all database's tables
                    foreach (var table in ImportedObjects)
                    {
                        if (table.ParentType != null)
                        {
                            if (table.ParentType.ToUpper() == database.Type)
                            {
                                if (table.ParentName == database.Name)
                                {
                                    Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");

                                    // print all table's columns
                                    foreach (var column in ImportedObjects)
                                    {
                                        if (column.ParentType != null)
                                        {
                                            if (column.ParentType.ToUpper() == table.Type)
                                            {
                                                if (column.ParentName == table.Name)
                                                {
                                                    Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("No columns found in the table.");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("No tables found in the database.");
                        }
                    }
                }
            }
            Console.ReadLine();
        }

        public bool AreAllPropertiesNotNull(object obj)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                if (property.GetValue(obj) == null)
                {
                    return false;
                }
            }
            return true;
        }
    }

    class ImportedObject : ImportedObjectBaseClass
    {
        public new string Name{ get; set; }
        public string Schema;

        public string ParentName;
        public string ParentType { get; set; }

        public string DataType { get; set; }
        public string IsNullable { get; set; }

        public double NumberOfChildren;
    }

    class ImportedObjectBaseClass
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
