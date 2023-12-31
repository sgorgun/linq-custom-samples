using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LinqSamples.Attributes;
using LinqSamples.Comparers;
using LinqSamples.NorthwindEntities;

namespace LinqSamples
{
    [Title("LINQ Query Samples")]
    [Prefix("Linq")]

    public class CustomSamples : SampleHarness
    {
        private readonly string dataPath;

        private List<Product> productList;
        private List<Customer> customerList;
        private List<Supplier> supplierList;

        public CustomSamples(string path)
        {
            this.dataPath = path ?? throw new ArgumentNullException(nameof(path));
        }

        [Category("Customers reports.")]
        [Title("List of customers by total orders.")]
        [Description("Report a list of all customers whose last all orders exceed some given value.")]
        public void LinqQuery01()
        {
            decimal limit = 10000.0M;

            var customers = this.GetCustomerList();
            var filteredCustomers = customers
                .Where(c => c.Orders != null && c.Orders.Sum(o => o.Total) > limit)
                .Select(c => new
                {
                    c.CustomerId,
                    c.CompanyName,
                });

            foreach (var customer in filteredCustomers)
            {
                Console.WriteLine($"Customer ID: {customer.CustomerId} | Customer: {customer.CompanyName}");
            }
        }

        [Category("Customers reports.")]
        [Title("List of customers by orders more than...")]
        [Description("Get a list of those customers whose orders exceed the specified amount.")]
        public void LinqQuery03()
        {
            decimal limit = 2000.0M;

            var customers = this.GetCustomerList();
            var filteredCustomers = customers
                .Where(c => c.Orders != null && c.Orders.Any(o => o.Total > limit))
                .Select(c => new
                {
                    c.CustomerId,
                    c.CompanyName,
                });

            foreach (var customer in filteredCustomers)
            {
                Console.WriteLine($"CustomerID: {customer.CustomerId} | Customer: {customer.CompanyName}");
            }
        }

        [Category("Customers reports.")]
        [Title("Sorted customers by year.")]
        [Description("Get a list of all customers sorted by year, month of the customer's first order, customer turnover (from maximum to minimum) and customer name.")]
        public void LinqQuery04()
        {
            var customers = this.GetCustomerList();
            var filteredCustomers = customers
                .Where(c => c.Orders != null && c.Orders.Length > 0)
                .Select(c => new
                {
                    c.CustomerId,
                    c.CompanyName,
                    c.Orders[0].OrderDate.Year,
                    c.Orders[0].OrderDate.Month,
                    Turnover = c.Orders.Sum(o => o.Total),
                })
                .OrderBy(c => c.Year)
                .ThenBy(c => c.Month)
                .ThenByDescending(c => c.Turnover)
                .ThenBy(c => c.CompanyName);

            foreach (var customer in filteredCustomers)
            {
                Console.WriteLine($"CustomerID: {customer.CustomerId} | Customer: {customer.CompanyName} | Year: {customer.Year} | Month: {customer.Month} | Turnover: {customer.Turnover}");
            }
        }

        [Category("Customers reports.")]
        [Title("Groupe by cityes (Average ordera amount & count).")]
        [Description("Calculate the average order amount for all customers from a given city and the average number of orders per customer from each city.")]
        public void LinqQuery08()
        {
            var customers = this.GetCustomerList();
            var filteredCustomers = customers
                .Where(c => c.Orders != null && c.Orders.Length > 0)
                .GroupBy(c => c.City)
                .Select(g => new
                {
                    City = g.Key,
                    AverageOrderAmount = g.Average(c => c.Orders.Sum(o => o.Total)),
                    AverageOrdersCount = g.Average(c => c.Orders.Length),
                });

            foreach (var customer in filteredCustomers)
            {
                Console.WriteLine($"City: {customer.City} | AverageOrderAmount: {customer.AverageOrderAmount.ToString("C", CultureInfo.InvariantCulture)} | AverageOrdersCount: {customer.AverageOrdersCount}");
            }
        }

        [Category("Customers reports.")]
        [Title("Best coutries, city, customers.")]
        [Description("Group orders by country, then by city, then by customer and sort all groupings and orders in descending order. In each grouping, enter the amount of orders.")]
        public void LinqQuery09()
        {
            var customers = this.GetCustomerList();
            var filteredCustomers = customers
                .Where(c => c.Orders != null && c.Orders.Length > 0)
                .GroupBy(c => c.Country)
                .Select(g => new
                {
                    Country = g.Key,
                    TotalAmount = g.Sum(c => c.Orders.Sum(o => o.Total)),
                    Cities = g.GroupBy(c => c.City)
                        .Select(g2 => new
                        {
                            City = g2.Key,
                            TotalAmount = g2.Sum(c => c.Orders.Sum(o => o.Total)),
                            Customers = g2.Select(c => new
                            {
                                CustomerName = c.CompanyName,
                                OrdersCount = c.Orders.Length,
                                TotalAmount = c.Orders.Sum(o => o.Total),
                                c.CustomerId,
                            })
                           .OrderByDescending(c => c.TotalAmount)
                           .ThenBy(c => c.CustomerName),
                        })
                        .OrderByDescending(c => c.TotalAmount),
                })
                .OrderByDescending(c => c.TotalAmount);

            foreach (var customer in filteredCustomers)
            {
                Console.WriteLine($"Country: {customer.Country} | Total Amount: {customer.TotalAmount}");

                foreach (var city in customer.Cities)
                {
                    Console.WriteLine($"City: {city.City} | Total Amount: {city.TotalAmount}");

                    foreach (var customer2 in city.Customers)
                    {
                        Console.WriteLine($"CustomerId: {customer2.CustomerId} | CustomerName: {customer2.CustomerName} | OrdersCount: {customer2.OrdersCount} | Total Amount: {customer2.TotalAmount}");
                    }
                }
            }
        }

        [Category("Customers reports.")]
        [Title("Rating by the largest average check.")]
        [Description("List your customers in descending order based on the size of the customer's average check (total sales divided by the number of sales.")]
        public void LinqQuery10()
        {
            var customers = this.GetCustomerList();
            var filteredCustomers = customers
                .Where(c => c.Orders != null && c.Orders.Length > 0)
                .Select(c => new
                {
                    c.CustomerId,
                    c.CompanyName,
                    AverageCheck = c.Orders.Average(o => o.Total),
                })
                .OrderByDescending(c => c.AverageCheck);

            foreach (var customer in filteredCustomers)
            {
                Console.WriteLine($"CustomerID: {customer.CustomerId} | Customer: {customer.CompanyName} | AverageCheck: {customer.AverageCheck.ToString("C", CultureInfo.InvariantCulture)}");
            }
        }

        [Category("Customers reports.")]
        [Title("Customers ABC analysis.")]
        [Description("Sort customers by sales amount. Sort the report into groups A, B, C. According to the following rules: Group A — The top 15% represent 70% of sales. Group B — The middle 20% represents 20% of sales. Group C — The bottom 65% represents 10% of sales.")]
        public void LinqQuery11()
        {
            var customers = this.GetCustomerList();
            var filteredCustomers = customers
                .Where(c => c.Orders != null && c.Orders.Length > 0)
                .Select(c => new
                {
                    c.CustomerId,
                    c.CompanyName,
                    TotalAmount = c.Orders.Sum(o => o.Total),
                })
                .OrderByDescending(c => c.TotalAmount)
                .ToArray();

            int aCount = (int)Math.Ceiling(filteredCustomers.Length * 0.15);
            int bCount = (int)Math.Ceiling(filteredCustomers.Length * 0.2);
            int cCount = filteredCustomers.Length - aCount - bCount;

            var aCustomers = filteredCustomers.Take(aCount);
            var bCustomers = filteredCustomers.Skip(aCount).Take(bCount);
            var cCustomers = filteredCustomers.Skip(aCount + bCount).Take(cCount);

            Console.WriteLine("Group A:");
            foreach (var customer in aCustomers)
            {
                Console.WriteLine($"CustomerID: {customer.CustomerId} | Customer: {customer.CompanyName} | TotalAmount: {customer.TotalAmount.ToString("C", CultureInfo.InvariantCulture)}");
            }

            Console.WriteLine("Group B:");
            foreach (var customer in bCustomers)
            {
                Console.WriteLine($"CustomerID: {customer.CustomerId} | Customer: {customer.CompanyName} | TotalAmount: {customer.TotalAmount.ToString("C", CultureInfo.InvariantCulture)}");
            }

            Console.WriteLine("Group C:");
            foreach (var customer in cCustomers)
            {
                Console.WriteLine($"CustomerID: {customer.CustomerId} | Customer: {customer.CompanyName} | TotalAmount: {customer.TotalAmount.ToString("C", CultureInfo.InvariantCulture)}");
            }
        }

        [Category("Customers reports.")]
        [Title("Lost customers.")]
        [Description("Output a list of customers who have not made a purchase after the specified date. Sort them in descending order by the number of days during which they have not made a purchase.")]
        public void LinqQuery13()
        {
            DateTime limit = new DateTime(1999, 1, 1);
            var customers = this.GetCustomerList();
            var filteredCustomers = customers
                .Where(c => c.Orders != null && c.Orders.Length > 0 && c.Orders.All(o => o.OrderDate < limit))
                .Select(c => new
                {
                    c.CustomerId,
                    c.CompanyName,
                    LastOrderDate = c.Orders.Max(o => o.OrderDate),
                    DaysSinceLastOrder = (DateTime.Now - c.Orders.Max(o => o.OrderDate)).Days,
                })
                .OrderByDescending(c => c.DaysSinceLastOrder);

            foreach (var customer in filteredCustomers)
            {
                Console.WriteLine($"CustomerID: {customer.CustomerId} | Customer: {customer.CompanyName} | LastOrderDate: {customer.LastOrderDate} | DaysSinceLastOrder: {customer.DaysSinceLastOrder}");
            }
        }

        [Category("Supplies reports.")]
        [Title("List of supplies by customer's sity.")]
        [Description("For each customer, get a list of suppliers located in the same country and the same city. The task can be performed both using the grouping operation and without it.")]
        public void LinqQuery02()
        {
            List<Customer> customers = this.GetCustomerList();
            List<Supplier> suppliers = this.GetSupplierList();

            var customerSuppliers = customers
                .GroupJoin(suppliers!, customer => (customer.Country, customer.City), sup => (sup.Country, sup.City), (customer, supplier) => (customer, supplier))
                .SelectMany(x => x.supplier.DefaultIfEmpty(), (x, supplier) => (x.customer, supplier))
                .OrderBy(x => x.customer.CompanyName)
                .Select(x => new
                {
                    x.customer.Country,
                    x.customer.City,
                    x.customer.CompanyName,
                    SupplierName = x.supplier == null ? "(No suppliers)" : x.supplier.SupplierName,
                });

            foreach (var item in customerSuppliers)
            {
                Console.WriteLine($"{item.CompanyName} ({item.Country}, {item.City}): {item.SupplierName}");
            }
        }

        [Category("Clients reports.")]
        [Title("Wrong clients data.")]
        [Description("Get a list of those customers with a non-numeric postal code or region not filled in, or no operator code in the phone (which equals \"no parentheses at the beginning\").")]
        public void LinqQuery05()
        {
            var customers = this.GetCustomerList();
            var filteredCustomers = customers
                .Where(c => (!string.IsNullOrEmpty(c.PostalCode) && !c.PostalCode.All(char.IsDigit)) || string.IsNullOrEmpty(c.Region) || !c.Phone.StartsWith('('));

            foreach (var customer in filteredCustomers)
            {
                Console.WriteLine($"CustomerID: {customer.CustomerId} | Customer: {customer.CompanyName} | PostalCode: {customer.PostalCode} | Region: {customer.Region} | Phone: {customer.Phone}");
            }
        }

        [Category("Commodity reports.")]
        [Title("Product groups by category.")]
        [Description("Group all products by category, within by stock availability, within the last group by cost.")]
        public void LinqQuery06()
        {
            var products = this.GetProductList();
            var sortedProducts = products
                .GroupBy(p => p.Category)
                .SelectMany(g => g.GroupBy(p => p.UnitsInStock > 0)
                    .Select(g2 => new
                    {
                        Category = g.Key,
                        UnitsInStock = g2.Key,
                        Products = g2.OrderBy(p => p.UnitPrice),
                    }));

            foreach (var product in sortedProducts)
            {
                Console.WriteLine($"Category: {product.Category}");
                Console.WriteLine($"UnitsInStock: {product.UnitsInStock}");

                foreach (var item in product.Products)
                {
                    Console.WriteLine($"Product: {item.ProductName} | Price: {item.UnitPrice.ToString("C", CultureInfo.InvariantCulture)}");
                }
            }
        }

        [Category("Commodity reports.")]
        [Title("Product groups by cost.")]
        [Description("Group all goods into groups \"cheap\", \"average price\", \"expensive\", defining the boundaries of each group arbitrarily.")]
        public void LinqQuery07()
        {
            var products = this.GetProductList();
            decimal cheapPrice = 20.0M;
            decimal averagePrice = 50.0M;
            var sortedProducts = products
                .GroupBy(p => p.UnitPrice < cheapPrice ? "Cheap" : p.UnitPrice < averagePrice ? "Average" : "Expensive")
                .SelectMany(g => g.OrderBy(p => p.UnitPrice).Select(p => new
                {
                    PriceGroup = g.Key,
                    Product = p,
                }));

            foreach (var product in sortedProducts)
            {
                Console.WriteLine($"PriceGroup: {product.PriceGroup}");
                Console.WriteLine($"Product: {product.Product.ProductName} | Price: {product.Product.UnitPrice.ToString("C", CultureInfo.InvariantCulture)}");
            }
        }

        [Category("Commodity reports.")]
        [Title("Analyzing the stock of goods in the warehouse.")]
        [Description("Output a list of products whose remaining quantity is less than the specified value. The list is sorted by increasing quantity.")]
        public void LinqQuery12()
        {
            int limit = 10;
            var products = this.GetProductList();
            var filteredProducts = products
                .Where(p => p.UnitsInStock < limit)
                .OrderBy(p => p.UnitsInStock);

            foreach (var product in filteredProducts)
            {
                Console.WriteLine($"Product: {product.ProductName} | UnitsInStock: {product.UnitsInStock}");
            }
        }

        private List<Product> GetProductList()
        {
            if (this.productList is null)
            {
                this.CreateLists();
            }

            return this.productList;
        }

        private List<Supplier> GetSupplierList()
        {
            if (this.supplierList is null)
            {
                this.CreateLists();
            }

            return this.supplierList;
        }

        private List<Customer> GetCustomerList()
        {
            if (this.customerList is null)
            {
                this.CreateLists();
            }

            return this.customerList;
        }

        private void CreateLists()
        {
            this.productList = new List<Product>
            {
                new Product
                {
                    ProductId = 1,
                    ProductName = "Chai",
                    Category = "Beverages",
                    UnitPrice = 18.0000M,
                    UnitsInStock = 39,
                },
                new Product
                {
                    ProductId = 2,
                    ProductName = "Chang",
                    Category = "Beverages",
                    UnitPrice = 19.0000M,
                    UnitsInStock = 17,
                },
                new Product
                {
                    ProductId = 3,
                    ProductName = "Aniseed Syrup",
                    Category = "Condiments",
                    UnitPrice = 10.0000M,
                    UnitsInStock = 13,
                },
                new Product
                {
                    ProductId = 4,
                    ProductName = "Chef Anton's Cajun Seasoning",
                    Category = "Condiments",
                    UnitPrice = 22.0000M,
                    UnitsInStock = 53,
                },
                new Product
                {
                    ProductId = 5,
                    ProductName = "Chef Anton's Gumbo Mix",
                    Category = "Condiments",
                    UnitPrice = 21.3500M,
                    UnitsInStock = 0,
                },
                new Product
                {
                    ProductId = 6,
                    ProductName = "Grandma's Boysenberry Spread",
                    Category = "Condiments",
                    UnitPrice = 25.0000M,
                    UnitsInStock = 120,
                },
                new Product
                {
                    ProductId = 7,
                    ProductName = "Uncle Bob's Organic Dried Pears",
                    Category = "Produce",
                    UnitPrice = 30.0000M,
                    UnitsInStock = 15,
                },
                new Product
                {
                    ProductId = 8,
                    ProductName = "Northwoods Cranberry Sauce",
                    Category = "Condiments",
                    UnitPrice = 40.0000M,
                    UnitsInStock = 6,
                },
                new Product
                {
                    ProductId = 9,
                    ProductName = "Mishi Kobe Niku",
                    Category = "Meat/Poultry",
                    UnitPrice = 97.0000M,
                    UnitsInStock = 29,
                },
                new Product
                {
                    ProductId = 10,
                    ProductName = "Ikura",
                    Category = "Seafood",
                    UnitPrice = 31.0000M,
                    UnitsInStock = 31,
                },
                new Product
                {
                    ProductId = 11,
                    ProductName = "Queso Cabrales",
                    Category = "Dairy Products",
                    UnitPrice = 21.0000M,
                    UnitsInStock = 22,
                },
                new Product
                {
                    ProductId = 12,
                    ProductName = "Queso Manchego La Pastora",
                    Category = "Dairy Products",
                    UnitPrice = 38.0000M,
                    UnitsInStock = 86,
                },
                new Product
                {
                    ProductId = 13,
                    ProductName = "Konbu",
                    Category = "Seafood",
                    UnitPrice = 6.0000M,
                    UnitsInStock = 24,
                },
                new Product
                {
                    ProductId = 14,
                    ProductName = "Tofu",
                    Category = "Produce",
                    UnitPrice = 23.2500M,
                    UnitsInStock = 35,
                },
                new Product
                {
                    ProductId = 15,
                    ProductName = "Genen Shouyu",
                    Category = "Condiments",
                    UnitPrice = 15.5000M,
                    UnitsInStock = 39,
                },
                new Product
                {
                    ProductId = 16,
                    ProductName = "Pavlova",
                    Category = "Confections",
                    UnitPrice = 17.4500M,
                    UnitsInStock = 29,
                },
                new Product
                {
                    ProductId = 17,
                    ProductName = "Alice Mutton",
                    Category = "Meat/Poultry",
                    UnitPrice = 39.0000M,
                    UnitsInStock = 0,
                },
                new Product
                {
                    ProductId = 18,
                    ProductName = "Carnarvon Tigers",
                    Category = "Seafood",
                    UnitPrice = 62.5000M,
                    UnitsInStock = 42,
                },
                new Product
                {
                    ProductId = 19,
                    ProductName = "Teatime Chocolate Biscuits",
                    Category = "Confections",
                    UnitPrice = 9.2000M,
                    UnitsInStock = 25,
                },
                new Product
                {
                    ProductId = 20,
                    ProductName = "Sir Rodney's Marmalade",
                    Category = "Confections",
                    UnitPrice = 81.0000M,
                    UnitsInStock = 40,
                },
                new Product
                {
                    ProductId = 21,
                    ProductName = "Sir Rodney's Scones",
                    Category = "Confections",
                    UnitPrice = 10.0000M,
                    UnitsInStock = 3,
                },
                new Product
                {
                    ProductId = 22,
                    ProductName = "Gustaf's Knäckebröd",
                    Category = "Grains/Cereals",
                    UnitPrice = 21.0000M,
                    UnitsInStock = 104,
                },
                new Product
                {
                    ProductId = 23,
                    ProductName = "Tunnbröd",
                    Category = "Grains/Cereals",
                    UnitPrice = 9.0000M,
                    UnitsInStock = 61,
                },
                new Product
                {
                    ProductId = 24,
                    ProductName = "Guaraná Fantástica",
                    Category = "Beverages",
                    UnitPrice = 4.5000M,
                    UnitsInStock = 20,
                },
                new Product
                {
                    ProductId = 25,
                    ProductName = "NuNuCa Nuß-Nougat-Creme",
                    Category = "Confections",
                    UnitPrice = 14.0000M,
                    UnitsInStock = 76,
                },
                new Product
                {
                    ProductId = 26,
                    ProductName = "Gumbär Gummibärchen",
                    Category = "Confections",
                    UnitPrice = 31.2300M,
                    UnitsInStock = 15,
                },
                new Product
                {
                    ProductId = 27,
                    ProductName = "Schoggi Schokolade",
                    Category = "Confections",
                    UnitPrice = 43.9000M,
                    UnitsInStock = 49,
                },
                new Product
                {
                    ProductId = 28,
                    ProductName = "Rössle Sauerkraut",
                    Category = "Produce",
                    UnitPrice = 45.6000M,
                    UnitsInStock = 26,
                },
                new Product
                {
                    ProductId = 29,
                    ProductName = "Thüringer Rostbratwurst",
                    Category = "Meat/Poultry",
                    UnitPrice = 123.7900M,
                    UnitsInStock = 0,
                },
                new Product
                {
                    ProductId = 30,
                    ProductName = "Nord-Ost Matjeshering",
                    Category = "Seafood",
                    UnitPrice = 25.8900M,
                    UnitsInStock = 10,
                },
                new Product
                {
                    ProductId = 31,
                    ProductName = "Gorgonzola Telino",
                    Category = "Dairy Products",
                    UnitPrice = 12.5000M,
                    UnitsInStock = 0,
                },
                new Product
                {
                    ProductId = 32,
                    ProductName = "Mascarpone Fabioli",
                    Category = "Dairy Products",
                    UnitPrice = 32.0000M,
                    UnitsInStock = 9,
                },
                new Product
                {
                    ProductId = 33,
                    ProductName = "Geitost",
                    Category = "Dairy Products",
                    UnitPrice = 2.5000M,
                    UnitsInStock = 112,
                },
                new Product
                {
                    ProductId = 34,
                    ProductName = "Sasquatch Ale",
                    Category = "Beverages",
                    UnitPrice = 14.0000M,
                    UnitsInStock = 111,
                },
                new Product
                {
                    ProductId = 35,
                    ProductName = "Steeleye Stout",
                    Category = "Beverages",
                    UnitPrice = 18.0000M,
                    UnitsInStock = 20,
                },
                new Product
                {
                    ProductId = 36,
                    ProductName = "Inlagd Sill",
                    Category = "Seafood",
                    UnitPrice = 19.0000M,
                    UnitsInStock = 112,
                },
                new Product
                {
                    ProductId = 37,
                    ProductName = "Gravad lax",
                    Category = "Seafood",
                    UnitPrice = 26.0000M,
                    UnitsInStock = 11,
                },
                new Product
                {
                    ProductId = 38,
                    ProductName = "Côte de Blaye",
                    Category = "Beverages",
                    UnitPrice = 263.5000M,
                    UnitsInStock = 17,
                },
                new Product
                {
                    ProductId = 39,
                    ProductName = "Chartreuse verte",
                    Category = "Beverages",
                    UnitPrice = 18.0000M,
                    UnitsInStock = 69,
                },
                new Product
                {
                    ProductId = 40,
                    ProductName = "Boston Crab Meat",
                    Category = "Seafood",
                    UnitPrice = 18.4000M,
                    UnitsInStock = 123,
                },
                new Product
                {
                    ProductId = 41,
                    ProductName = "Jack's New England Clam Chowder",
                    Category = "Seafood",
                    UnitPrice = 9.6500M,
                    UnitsInStock = 85,
                },
                new Product
                {
                    ProductId = 42,
                    ProductName = "Singaporean Hokkien Fried Mee",
                    Category = "Grains/Cereals",
                    UnitPrice = 14.0000M,
                    UnitsInStock = 26,
                },
                new Product
                {
                    ProductId = 43,
                    ProductName = "Ipoh Coffee",
                    Category = "Beverages",
                    UnitPrice = 46.0000M,
                    UnitsInStock = 17,
                },
                new Product
                {
                    ProductId = 44,
                    ProductName = "Gula Malacca",
                    Category = "Condiments",
                    UnitPrice = 19.4500M,
                    UnitsInStock = 27,
                },
                new Product
                {
                    ProductId = 45,
                    ProductName = "Rogede sild",
                    Category = "Seafood",
                    UnitPrice = 9.5000M,
                    UnitsInStock = 5,
                },
                new Product
                {
                    ProductId = 46,
                    ProductName = "Spegesild",
                    Category = "Seafood",
                    UnitPrice = 12.0000M,
                    UnitsInStock = 95,
                },
                new Product
                {
                    ProductId = 47,
                    ProductName = "Zaanse koeken",
                    Category = "Confections",
                    UnitPrice = 9.5000M,
                    UnitsInStock = 36,
                },
                new Product
                {
                    ProductId = 48,
                    ProductName = "Chocolade",
                    Category = "Confections",
                    UnitPrice = 12.7500M,
                    UnitsInStock = 15,
                },
                new Product
                {
                    ProductId = 49,
                    ProductName = "Maxilaku",
                    Category = "Confections",
                    UnitPrice = 20.0000M,
                    UnitsInStock = 10,
                },
                new Product
                {
                    ProductId = 50,
                    ProductName = "Valkoinen suklaa",
                    Category = "Confections",
                    UnitPrice = 16.2500M,
                    UnitsInStock = 65,
                },
                new Product
                {
                    ProductId = 51,
                    ProductName = "Manjimup Dried Apples",
                    Category = "Produce",
                    UnitPrice = 53.0000M,
                    UnitsInStock = 20,
                },
                new Product
                {
                    ProductId = 52,
                    ProductName = "Filo Mix",
                    Category = "Grains/Cereals",
                    UnitPrice = 7.0000M,
                    UnitsInStock = 38,
                },
                new Product
                {
                    ProductId = 53,
                    ProductName = "Perth Pasties",
                    Category = "Meat/Poultry",
                    UnitPrice = 32.8000M,
                    UnitsInStock = 0,
                },
                new Product
                {
                    ProductId = 54,
                    ProductName = "Tourtière",
                    Category = "Meat/Poultry",
                    UnitPrice = 7.4500M,
                    UnitsInStock = 21,
                },
                new Product
                {
                    ProductId = 55,
                    ProductName = "Pâté chinois",
                    Category = "Meat/Poultry",
                    UnitPrice = 24.0000M,
                    UnitsInStock = 115,
                },
                new Product
                {
                    ProductId = 56,
                    ProductName = "Gnocchi di nonna Alice",
                    Category = "Grains/Cereals",
                    UnitPrice = 38.0000M,
                    UnitsInStock = 21,
                },
                new Product
                {
                    ProductId = 57,
                    ProductName = "Ravioli Angelo",
                    Category = "Grains/Cereals",
                    UnitPrice = 19.5000M,
                    UnitsInStock = 36,
                },
                new Product
                {
                    ProductId = 58,
                    ProductName = "Escargots de Bourgogne",
                    Category = "Seafood",
                    UnitPrice = 13.2500M,
                    UnitsInStock = 62,
                },
                new Product
                {
                    ProductId = 59,
                    ProductName = "Raclette Courdavault",
                    Category = "Dairy Products",
                    UnitPrice = 55.0000M,
                    UnitsInStock = 79,
                },
                new Product
                {
                    ProductId = 60,
                    ProductName = "Camembert Pierrot",
                    Category = "Dairy Products",
                    UnitPrice = 34.0000M,
                    UnitsInStock = 19,
                },
                new Product
                {
                    ProductId = 61,
                    ProductName = "Sirop d'érable",
                    Category = "Condiments",
                    UnitPrice = 28.5000M,
                    UnitsInStock = 113,
                },
                new Product
                {
                    ProductId = 62,
                    ProductName = "Tarte au sucre",
                    Category = "Confections",
                    UnitPrice = 49.3000M,
                    UnitsInStock = 17,
                },
                new Product
                {
                    ProductId = 63,
                    ProductName = "Vegie-spread",
                    Category = "Condiments",
                    UnitPrice = 43.9000M,
                    UnitsInStock = 24,
                },
                new Product
                {
                    ProductId = 64,
                    ProductName = "Wimmers gute Semmelknödel",
                    Category = "Grains/Cereals",
                    UnitPrice = 33.2500M,
                    UnitsInStock = 22,
                },
                new Product
                {
                    ProductId = 65,
                    ProductName = "Louisiana Fiery Hot Pepper Sauce",
                    Category = "Condiments",
                    UnitPrice = 21.0500M,
                    UnitsInStock = 76,
                },
                new Product
                {
                    ProductId = 66,
                    ProductName = "Louisiana Hot Spiced Okra",
                    Category = "Condiments",
                    UnitPrice = 17.0000M,
                    UnitsInStock = 4,
                },
                new Product
                {
                    ProductId = 67,
                    ProductName = "Laughing Lumberjack Lager",
                    Category = "Beverages",
                    UnitPrice = 14.0000M,
                    UnitsInStock = 52,
                },
                new Product
                {
                    ProductId = 68,
                    ProductName = "Scottish Longbreads",
                    Category = "Confections",
                    UnitPrice = 12.5000M,
                    UnitsInStock = 6,
                },
                new Product
                {
                    ProductId = 69,
                    ProductName = "Gudbrandsdalsost",
                    Category = "Dairy Products",
                    UnitPrice = 36.0000M,
                    UnitsInStock = 26,
                },
                new Product
                {
                    ProductId = 70,
                    ProductName = "Outback Lager",
                    Category = "Beverages",
                    UnitPrice = 15.0000M,
                    UnitsInStock = 15,
                },
                new Product
                {
                    ProductId = 71,
                    ProductName = "Flotemysost",
                    Category = "Dairy Products",
                    UnitPrice = 21.5000M,
                    UnitsInStock = 26,
                },
                new Product
                {
                    ProductId = 72,
                    ProductName = "Mozzarella di Giovanni",
                    Category = "Dairy Products",
                    UnitPrice = 34.8000M,
                    UnitsInStock = 14,
                },
                new Product
                {
                    ProductId = 73,
                    ProductName = "Röd Kaviar",
                    Category = "Seafood",
                    UnitPrice = 15.0000M,
                    UnitsInStock = 101,
                },
                new Product
                {
                    ProductId = 74,
                    ProductName = "Longlife Tofu",
                    Category = "Produce",
                    UnitPrice = 10.0000M,
                    UnitsInStock = 4,
                },
                new Product
                {
                    ProductId = 75,
                    ProductName = "Rhönbräu Klosterbier",
                    Category = "Beverages",
                    UnitPrice = 7.7500M,
                    UnitsInStock = 125,
                },
                new Product
                {
                    ProductId = 76,
                    ProductName = "Lakkalikööri",
                    Category = "Beverages",
                    UnitPrice = 18.0000M,
                    UnitsInStock = 57,
                },
                new Product
                {
                    ProductId = 77,
                    ProductName = "Original Frankfurter grüne Soße",
                    Category = "Condiments",
                    UnitPrice = 13.0000M,
                    UnitsInStock = 32,
                },
            };

            this.supplierList = new List<Supplier>()
            {
                new Supplier
                {
                    SupplierName = "Exotic Liquids",
                    Address = "49 Gilbert St.",
                    City = "London",
                    Country = "UK",
                },
                new Supplier
                {
                    SupplierName = "New Orleans Cajun Delights",
                    Address = "P.O. Box 78934",
                    City = "New Orleans",
                    Country = "USA",
                },
                new Supplier
                {
                    SupplierName = "Grandma Kelly's Homestead",
                    Address = "707 Oxford Rd.",
                    City = "Ann Arbor",
                    Country = "USA",
                },
                new Supplier
                {
                    SupplierName = "Tokyo Traders",
                    Address = "9-8 Sekimai Musashino-shi",
                    City = "Tokyo",
                    Country = "Japan",
                },
                new Supplier
                {
                    SupplierName = "Cooperativa de Quesos 'Las Cabras'",
                    Address = "Calle del Rosal 4",
                    City = "Oviedo",
                    Country = "Spain",
                },
                new Supplier
                {
                    SupplierName = "Mayumi's",
                    Address = "92 Setsuko Chuo-ku",
                    City = "Osaka",
                    Country = "Japan",
                },
                new Supplier
                {
                    SupplierName = "Pavlova, Ltd.",
                    Address = "74 Rose St. Moonie Ponds",
                    City = "Melbourne",
                    Country = "Australia",
                },
                new Supplier
                {
                    SupplierName = "Specialty Biscuits, Ltd.",
                    Address = "29 King's Way",
                    City = "Manchester",
                    Country = "UK",
                },
                new Supplier
                {
                    SupplierName = "PB Knäckebröd AB",
                    Address = "Kaloadagatan 13",
                    City = "Göteborg",
                    Country = "Sweden",
                },
                new Supplier
                {
                    SupplierName = "Refrescos Americanas LTDA",
                    Address = "Av. das Americanas 12.890",
                    City = "Sao Paulo",
                    Country = "Brazil",
                },
                new Supplier
                {
                    SupplierName = "Heli Süßwaren GmbH & Co. KG",
                    Address = "Tiergartenstraße 5",
                    City = "Berlin",
                    Country = "Germany",
                },
                new Supplier
                {
                    SupplierName = "Plutzer Lebensmittelgroßmärkte AG",
                    Address = "Bogenallee 51",
                    City = "Frankfurt",
                    Country = "Germany",
                },
                new Supplier
                {
                    SupplierName = "Nord-Ost-Fisch Handelsgesellschaft mbH",
                    Address = "Frahmredder 112a",
                    City = "Cuxhaven",
                    Country = "Germany",
                },
                new Supplier
                {
                    SupplierName = "Formaggi Fortini s.r.l.",
                    Address = "Viale Dante, 75",
                    City = "Ravenna",
                    Country = "Italy",
                },
                new Supplier
                {
                    SupplierName = "Norske Meierier",
                    Address = "Hatlevegen 5",
                    City = "Sandvika",
                    Country = "Norway",
                },
                new Supplier
                {
                    SupplierName = "Bigfoot Breweries",
                    Address = "3400 - 8th Avenue Suite 210",
                    City = "Bend",
                    Country = "USA",
                },
                new Supplier
                {
                    SupplierName = "Svensk Sjöföda AB",
                    Address = "Brovallavägen 231",
                    City = "Stockholm",
                    Country = "Sweden",
                },
                new Supplier
                {
                    SupplierName = "Aux joyeux ecclésiastiques",
                    Address = "203, Rue des Francs-Bourgeois",
                    City = "Paris",
                    Country = "France",
                },
                new Supplier
                {
                    SupplierName = "New England Seafood Cannery",
                    Address = "Order Processing Dept. 2100 Paul Revere Blvd.",
                    City = "Boston",
                    Country = "USA",
                },
                new Supplier
                {
                    SupplierName = "Leka Trading",
                    Address = "471 Serangoon Loop, Suite #402",
                    City = "Singapore",
                    Country = "Singapore",
                },
                new Supplier
                {
                    SupplierName = "Lyngbysild",
                    Address = "Lyngbysild Fiskebakken 10",
                    City = "Lyngby",
                    Country = "Denmark",
                },
                new Supplier
                {
                    SupplierName = "Zaanse Snoepfabriek",
                    Address = "Verkoop Rijnweg 22",
                    City = "Zaandam",
                    Country = "Netherlands",
                },
                new Supplier
                {
                    SupplierName = "Karkki Oy",
                    Address = "Valtakatu 12",
                    City = "Lappeenranta",
                    Country = "Finland",
                },
                new Supplier
                {
                    SupplierName = "G'day, Mate",
                    Address = "170 Prince Edward Parade Hunter's Hill",
                    City = "Sydney",
                    Country = "Australia",
                },
                new Supplier
                {
                    SupplierName = "Ma Maison",
                    Address = "2960 Rue St. Laurent",
                    City = "Montréal",
                    Country = "Canada",
                },
                new Supplier
                {
                    SupplierName = "Pasta Buttini s.r.l.",
                    Address = "Via dei Gelsomini, 153",
                    City = "Salerno",
                    Country = "Italy",
                },
                new Supplier
                {
                    SupplierName = "Escargots Nouveaux",
                    Address = "22, rue H. Voiron",
                    City = "Montceau",
                    Country = "France",
                },
                new Supplier
                {
                    SupplierName = "Gai pâturage",
                    Address = "Bat. B 3, rue des Alpes",
                    City = "Annecy",
                    Country = "France",
                },
                new Supplier
                {
                    SupplierName = "Forêts d'érables",
                    Address = "148 rue Chasseur",
                    City = "Ste-Hyacinthe",
                    Country = "Canada",
                },
            };

            string customerListPath = Path.GetFullPath(Path.Combine(this.dataPath, "customers.xml"));

            this.customerList = (
                from xElement in XDocument.Load(customerListPath).Root.Elements("customer")
                select new Customer
                {
                    CustomerId = (string)xElement.Element("id"),
                    CompanyName = (string)xElement.Element("name"),
                    Address = (string)xElement.Element("address"),
                    City = (string)xElement.Element("city"),
                    Region = (string)xElement.Element("region"),
                    PostalCode = (string)xElement.Element("postalcode"),
                    Country = (string)xElement.Element("country"),
                    Phone = (string)xElement.Element("phone"),
                    Fax = (string)xElement.Element("fax"),
                    Orders = (
                            from order in xElement.Elements("orders").Elements("order")
                            select new Order
                            {
                                OrderId = (int)order.Element("id"),
                                OrderDate = (DateTime)order.Element("orderdate"),
                                Total = (decimal)order.Element("total"),
                            })
                        .ToArray(),
                }).ToList();
        }
    }
}
