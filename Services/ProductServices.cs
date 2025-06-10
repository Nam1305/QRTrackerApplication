using DataAccess.Models;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class ProductServices
    {
        ProductRepo productRepo;

        public ProductServices()
        {
            productRepo = new ProductRepo();
        }

        public int GetTotalProductCount(string? keyword = null)
        {
            return productRepo.GetTotalProductCount(keyword);
        }

        public List<Product> LoadPagedProducts(int page, int pageSize, string? keyword = null)
        {
            return productRepo.LoadPagedProducts(page, pageSize, keyword);
        }

        public bool AddNewProduct(Product product) 
        {
            return productRepo.AddNewProduct(product);
        }

        public bool IsProductCodeExist(string productCode)
        {
            return productRepo.IsProductCodeExist(productCode);
        }

        public bool UpdateProduct(Product product) 
        {
            return productRepo.UpdateProduct(product);  
        }

        public Product? GetProductByCode(string productCode)
        {
            if (string.IsNullOrWhiteSpace(productCode))
                return null;
            return productRepo.GetProductByCode(productCode.Trim());
        }


    }
}
