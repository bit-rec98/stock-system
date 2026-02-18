using AutoMapper;
using StockSystem.Application.DTOs.Product;
using StockSystem.Application.DTOs.Category;
using StockSystem.Application.DTOs.Supplier;
using StockSystem.Domain.Entities;

namespace StockSystem.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product mappings
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name))
            .ForMember(d => d.SupplierName, opt => opt.MapFrom(s => s.Supplier != null ? s.Supplier.Name : null))
            .ForMember(d => d.IsLowStock, opt => opt.MapFrom(s => s.Quantity <= s.MinStockLevel));

        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();

        // Category mappings
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.ProductCount, opt => opt.MapFrom(s => s.Products.Count));
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>();

        // Supplier mappings
        CreateMap<Supplier, SupplierDto>()
            .ForMember(d => d.ProductCount, opt => opt.MapFrom(s => s.Products.Count));
        CreateMap<CreateSupplierDto, Supplier>();
        CreateMap<UpdateSupplierDto, Supplier>();
    }
}
