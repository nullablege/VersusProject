using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiyaslasana.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    user_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "bit", nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    security_stamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phone_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "bit", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "bit", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "bit", nullable: false),
                    access_failed_count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "telefonlar",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    model_adi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    marka = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    resim_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    network_teknolojisi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    duyurulma_tarihi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    duyurulma_tarihi_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    piyasaya_cikis_durumu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    piyasaya_cikis_durumu_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    boyutlar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    agirlik = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    govde_malzemesi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    govde_malzemesi_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sim = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sim_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    govde_diger = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    govde_diger_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ekran_tipi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ekran_boyutu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ekran_boyutu_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ekran_cozunurlugu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ekran_cozunurlugu_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ekran_korumasi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ekran_diger_ozellikler = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isletim_sistemi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isletim_sistemi_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    yonga_seti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cpu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gpu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gpu_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hafiza_karti_yuvasi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hafiza_karti_yuvasi_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dahili_hafiza = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ana_kamera_modulleri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ana_kamera_modulleri_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ana_kamera_ozellikleri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ana_kamera_ozellikleri_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ana_kamera_video = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ana_kamera_video_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    on_kamera_modulleri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    on_kamera_modulleri_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    on_kamera_ozellikleri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    on_kamera_ozellikleri_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    on_kamera_video = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    on_kamera_video_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hoparlor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hoparlor_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ses_3_5mm_jack = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ses_3_5mm_jack_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ses_diger_ozellikler = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    wlan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bluetooth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    konumlandirma_gps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nfc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nfc_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    kizilotesi_portu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    radyo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    radyo_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    usb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sensorler = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sensorler_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    batarya_tipi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sarj_ozellikleri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sarj_ozellikleri_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    batarya_diger = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    renkler = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    renkler_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    model_varyantlari = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sar_ab = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sar_ab_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sar_abd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sar_abd_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fiyat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    performans_testleri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    testler_ekran = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    testler_ekran_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    testler_kamera_link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    testler_hoparlor_link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    testler_batarya_omru_puan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    testler_batarya_omru_puan_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    testler_batarya_omru_detay_link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    testler_aktif_kullanim_skoru = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    testler_aktif_kullanim_skoru_tr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    review_durumu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    review_icerigi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    review_icerigi_turkce = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    son_guncelleme = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    kayit_tarihi = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    slug = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_telefonlar", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    claim_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    claim_value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "AspNetRoles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    claim_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    claim_value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_user_claims_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    provider_key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    provider_display_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_asp_net_user_logins_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    role_id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "AspNetRoles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    login_provider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_role_claims_role_id",
                table: "AspNetRoleClaims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "normalized_name",
                unique: true,
                filter: "[normalized_name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_claims_user_id",
                table: "AspNetUserClaims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_logins_user_id",
                table: "AspNetUserLogins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_roles_role_id",
                table: "AspNetUserRoles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "normalized_user_name",
                unique: true,
                filter: "[normalized_user_name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_telefonlar_slug",
                table: "telefonlar",
                column: "slug",
                unique: true,
                filter: "[slug] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "telefonlar");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
