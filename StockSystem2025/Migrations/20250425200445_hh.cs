using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockSystem2025.Migrations
{
    /// <inheritdoc />
    public partial class hh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyTable",
                columns: table => new
                {
                    CompanyCode = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Follow = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    IsSpecial = table.Column<bool>(type: "bit", nullable: false),
                    IsIndicator = table.Column<bool>(type: "bit", nullable: false),
                    ParentIndicator = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__tmp_ms_x__11A0134A8B5287C1", x => x.CompanyCode);
                    table.ForeignKey(
                        name: "FK_CompanyTable_CompanyTable",
                        column: x => x.ParentIndicator,
                        principalTable: "CompanyTable",
                        principalColumn: "CompanyCode");
                });

            migrationBuilder.CreateTable(
                name: "DigitalAnalysis",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TopValue = table.Column<double>(type: "float", nullable: false),
                    Bottom = table.Column<double>(type: "float", nullable: false),
                    WavesCount = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    WavesVisibility = table.Column<string>(type: "varchar(400)", unicode: false, maxLength: 400, nullable: true),
                    OrderNo = table.Column<int>(type: "int", nullable: true),
                    ShowDescriptionColumn = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CompanyCode = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: true),
                    TopValueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BottomValueDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalAnalysis", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "EconLinksTypes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EconLinksTypes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ProfessionalFibonacci",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TopValue = table.Column<double>(type: "float", nullable: false),
                    BottomValue = table.Column<double>(type: "float", nullable: false),
                    FibonacciPercentageValue = table.Column<double>(type: "float", nullable: false),
                    OrderNo = table.Column<int>(type: "int", nullable: true),
                    ShowDescriptionColumn = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CompanyCode = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: true),
                    TopValueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BottomValueDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessionalFibonacci", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "RefreshedPage",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PageURL = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshedPage", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Settings__3214EC2795A6AAB3", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "StockTable",
                columns: table => new
                {
                    Sticker = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: false),
                    Sdate = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DayNo = table.Column<int>(type: "int", nullable: false),
                    Sname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Sopen = table.Column<double>(type: "float", nullable: true),
                    SHigh = table.Column<double>(type: "float", nullable: true),
                    SLow = table.Column<double>(type: "float", nullable: true),
                    SClose = table.Column<double>(type: "float", nullable: true),
                    Svol = table.Column<double>(type: "float", nullable: true),
                    ExpectedOpen = table.Column<double>(type: "float", nullable: true),
                    Createddate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTable", x => new { x.Sticker, x.Sdate });
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValueSql: "((1))"),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValueSql: "((1))"),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmailCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    Bundle = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DigitalAnalysisData",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DigitalAnalysisID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    Color = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Visible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalAnalysisData", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DigitalAnalysisData_DigitalAnalysis",
                        column: x => x.DigitalAnalysisID,
                        principalTable: "DigitalAnalysis",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "EconomicLinks",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EconomicLinks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EconomicLinks_EconLinksTypes",
                        column: x => x.TypeID,
                        principalTable: "EconLinksTypes",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "ProfessionalFibonacciData",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfessionalFibonacciID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Value = table.Column<double>(type: "float", nullable: false),
                    Color = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Visible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessionalFibonacciData", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ProfessionalFibonacciData_ProfessionalFibonacci",
                        column: x => x.ProfessionalFibonacciID,
                        principalTable: "ProfessionalFibonacci",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Criterias",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "ntext", nullable: true),
                    Separater = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OrderNo = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Color = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    ImageURL = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    IsIndicator = table.Column<int>(type: "int", nullable: false, comment: "0  = false, 1 = true, 2 = all"),
                    IsGeneral = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    UserID = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Criterias", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Criterias_Users",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "FollowList",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Color = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    StopLoss = table.Column<double>(type: "float", nullable: true),
                    FirstSupport = table.Column<double>(type: "float", nullable: true),
                    SecondSupport = table.Column<double>(type: "float", nullable: true),
                    FirstTarget = table.Column<double>(type: "float", nullable: true),
                    SecondTarget = table.Column<double>(type: "float", nullable: true),
                    ThirdTarget = table.Column<double>(type: "float", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowList", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FollowList_Users",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Mediums",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DaysMedium = table.Column<int>(type: "int", nullable: false),
                    ForTable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ForChart = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    UserID = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mediums", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Mediums_Users",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Formulas",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormulaType = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    FormulaValues = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    CriteriaID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Formulas", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Formulas_Criterias",
                        column: x => x.CriteriaID,
                        principalTable: "Criterias",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "FollowListCompanies",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FollowListID = table.Column<int>(type: "int", nullable: true),
                    CompanyCode = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowListCompanies", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FollowListCompanies_CompanyTable",
                        column: x => x.CompanyCode,
                        principalTable: "CompanyTable",
                        principalColumn: "CompanyCode");
                    table.ForeignKey(
                        name: "FK_FollowListCompanies_FollowList",
                        column: x => x.FollowListID,
                        principalTable: "FollowList",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyTable_ParentIndicator",
                table: "CompanyTable",
                column: "ParentIndicator");

            migrationBuilder.CreateIndex(
                name: "IX_Criterias_UserID",
                table: "Criterias",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAnalysisData_DigitalAnalysisID",
                table: "DigitalAnalysisData",
                column: "DigitalAnalysisID");

            migrationBuilder.CreateIndex(
                name: "IX_EconomicLinks_TypeID",
                table: "EconomicLinks",
                column: "TypeID");

            migrationBuilder.CreateIndex(
                name: "IX_FollowList_UserID",
                table: "FollowList",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_FollowListCompanies_CompanyCode",
                table: "FollowListCompanies",
                column: "CompanyCode");

            migrationBuilder.CreateIndex(
                name: "IX_FollowListCompanies_FollowListID",
                table: "FollowListCompanies",
                column: "FollowListID");

            migrationBuilder.CreateIndex(
                name: "IX_Formulas_CriteriaID",
                table: "Formulas",
                column: "CriteriaID");

            migrationBuilder.CreateIndex(
                name: "IX_Mediums_UserID",
                table: "Mediums",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionalFibonacciData_ProfessionalFibonacciID",
                table: "ProfessionalFibonacciData",
                column: "ProfessionalFibonacciID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DigitalAnalysisData");

            migrationBuilder.DropTable(
                name: "EconomicLinks");

            migrationBuilder.DropTable(
                name: "FollowListCompanies");

            migrationBuilder.DropTable(
                name: "Formulas");

            migrationBuilder.DropTable(
                name: "Mediums");

            migrationBuilder.DropTable(
                name: "ProfessionalFibonacciData");

            migrationBuilder.DropTable(
                name: "RefreshedPage");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "StockTable");

            migrationBuilder.DropTable(
                name: "DigitalAnalysis");

            migrationBuilder.DropTable(
                name: "EconLinksTypes");

            migrationBuilder.DropTable(
                name: "CompanyTable");

            migrationBuilder.DropTable(
                name: "FollowList");

            migrationBuilder.DropTable(
                name: "Criterias");

            migrationBuilder.DropTable(
                name: "ProfessionalFibonacci");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
