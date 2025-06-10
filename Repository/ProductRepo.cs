using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class ProductRepo
    {
        private readonly NewDbContext context;

        public ProductRepo()
        {
            context = new NewDbContext();
        }

        public List<Product> LoadPagedProducts(int page, int pageSize, string? keyword = null)
        {
            var query = context.Products.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.ProductCode.Contains(keyword));
            }
            return query
                .OrderBy(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        //Lấy tổng số sản phẩm (dùng để tính tổng số trang)
        public int GetTotalProductCount(string? keyword = null)
        {
            var query = context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.ProductCode.Contains(keyword));
            }

            return query.Count();
        }

        //Thêm sản phẩm mới
        public bool AddNewProduct(Product product)
        {
            try
            {
                context.Products.Add(product);
                return context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool IsProductCodeExist(string productCode)
        {
            if (string.IsNullOrWhiteSpace(productCode))
                return false;
            return context.Products.Any(p =>p.ProductCode.ToLower() == productCode.Trim().ToLower());
        }

        public bool UpdateProduct(Product product)
        {
            try
            {
                var existingProduct = context.Products.FirstOrDefault(p => p.ProductCode == product.ProductCode);
                if (existingProduct != null)
                {
                    existingProduct.QuantityPerBox = product.QuantityPerBox;
                    existingProduct.QuantityPerTray = product.QuantityPerTray;
                    existingProduct.TrayPerBox = product.TrayPerBox;
                    return context.SaveChanges() > 0;

                }
                return false;
                
            }
            catch (Exception ex) 
            {
                return false;
            }
            
        }

        public Product? GetProductByCode(string code)
        {
            try
            {
                return context.Products.FirstOrDefault(p => p.ProductCode == code);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
