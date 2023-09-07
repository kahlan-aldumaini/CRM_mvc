﻿using CRM_mvc.Utilities.Enumerations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM_mvc.Migrations
{
    /// <inheritdoc />
    public partial class InitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        
    {
        migrationBuilder.InsertData("CallChannels", new[] { "Name", "CreatedAt", "UpdatedAt" },
            new object[]
            {
                ChatChannelEnum.SMS.ToString(),
                DateTime.Now,
                DateTime.Now,
            });
        migrationBuilder.InsertData("CallChannels", new[] { "Name", "CreatedAt", "UpdatedAt" },
            new object[]
            {
                ChatChannelEnum.EMAIL.ToString(),
                DateTime.Now,
                DateTime.Now,
            });
        migrationBuilder.InsertData("CallChannels", new[] { "Name", "CreatedAt", "UpdatedAt" },
            new object[]
            {
                ChatChannelEnum.SESSION.ToString(),
                DateTime.Now,
                DateTime.Now,
            });

        migrationBuilder.InsertData("CallTypes", new[] { "Name", "CreatedAt", "UpdatedAt" },
            new object[]
            {
                CallTypeEnum.OUTGOING.ToString(),
                DateTime.Now,
                DateTime.Now,
            });
        migrationBuilder.InsertData("CallTypes", new[] { "Name", "CreatedAt", "UpdatedAt" },
            new object[]
            {
                CallTypeEnum.INCOMING.ToString(),
                DateTime.Now,
                DateTime.Now,
            });
        
        migrationBuilder.InsertData("AspNetRoles", new[] {"Id", "Name" },
            new object[]
            {
                Guid.NewGuid().ToString(),
                MainRoles.USER.ToString(),
            });
        
        var adminRoleId = Guid.NewGuid().ToString();
        migrationBuilder.InsertData("AspNetRoles", new[] { "Id", "Name" },
            new object[]
            {
                adminRoleId,
                MainRoles.ADMIN.ToString(),
            });
        
        migrationBuilder.InsertData("AspNetRoles", new[] {"Id", "Name" },
            new object[]
            {
                Guid.NewGuid().ToString(),
                MainRoles.BACKOFFICE.ToString(),
            });

        migrationBuilder.InsertData("ReturnActions", new[] { "Title", "CreatedAt", "UpdatedAt" },
            new object[]
            {
                ReturnActionType.SMS.ToString(),
                DateTime.Now,
                DateTime.Now,
            });
        
        migrationBuilder.InsertData("ReturnActions", new[] { "Title", "CreatedAt", "UpdatedAt" },
            new object[]
            {
                ReturnActionType.EMAIL.ToString(),
                DateTime.Now,
                DateTime.Now,
            });
        
        migrationBuilder.InsertData("ReturnActions", new[] { "Title", "CreatedAt", "UpdatedAt" },
            new object[]
            {
                ReturnActionType.CALL.ToString(),
                DateTime.Now,
                DateTime.Now,
            });
        // add admin user 
        var adminId = Guid.NewGuid().ToString();

        migrationBuilder.InsertData("AspNetUsers", new[] { "Id", "Name","UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount", "CreatedAt", "UpdatedAt" },
            
                new object[]
               {
                   adminId,
                   "Admin",
                   "Admin",
                    "admin",
                   "admin@gmail.com",
                   "admin@gmail.com",
                  true,
                    "AQAAAAIAAYagAAAAENp4Edg5VI9HhIDfMaaUtMM27TyPPv5oYMwXdkChDhyZP66irurVLlPgT/7C3K5BTg==", // Admin@123456
                   "securitystamp",
                    "concurrencystamp",
                    false,
                    false,
                    false,
                   0,
                    DateTime.Now,
                    DateTime.Now,
                });
        
        migrationBuilder.InsertData("AspNetUserRoles",new[] {"UserId","RoleId"},
            new object[]
            {
                adminId,
                adminRoleId
            });



    }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
