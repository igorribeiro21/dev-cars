using DevCars.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevCars.API.Persistence.Configurations
{
    public class CarDbConfiguration : IEntityTypeConfiguration<Car>
    {
        public void Configure(EntityTypeBuilder<Car> builder)
        {
            builder
                .HasKey(c => c.Id);

            builder
                .Property(c => c.Brand)
                //.IsRequired()
                //.HasColumnName("Marca")
                .HasColumnType("VARCHAR(100)")
                .HasDefaultValue("PADRÃO")
                .HasMaxLength(100);

            builder
                .Property(c => c.ProductionDate)
                .HasDefaultValueSql("getdate()");

            //ToTable seria para instanciar o nome da tabela, se não passar por padrão será o nome da
            //propriedade por ex: Car
            //builder.Entity<Car>()
            //    .ToTable("tb_Car");
        }
    }
}
