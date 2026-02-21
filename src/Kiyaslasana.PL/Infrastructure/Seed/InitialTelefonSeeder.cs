using System.Text;
using Kiyaslasana.DAL.Data;
using Kiyaslasana.EL.Constants;
using Kiyaslasana.EL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kiyaslasana.PL.Infrastructure.Seed;

public sealed class InitialTelefonSeeder
{
    private static readonly IReadOnlyList<SeedTelefonRow> SeedRows =
    [
        new SeedTelefonRow
        {
            ModelAdi = "Xiaomi 14T Pro",
            Marka = "Xiaomi",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/xiaomi-14t-pro.jpg",
            NetworkTeknolojisi = "GSM / HSPA / LTE / 5G",
            DuyurulmaTarihi = "2024, September 26",
            PiyasayaCikisDurumu = "Available. Released 2024, September 26",
            EkranTipi = "AMOLED, 68B colors, 144Hz, Dolby Vision, HDR10+, 1600 nits (HBM), 4000 nits (peak)",
            EkranBoyutu = "6.67 inches, 107.4 cm2 (~89.1% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "Android 14, HyperOS",
            YongaSeti = "Mediatek Dimensity 9300+ (4 nm)",
            Cpu = "Octa-core (1x3.4 GHz Cortex-X4 & 3x2.85 GHz Cortex-X4 & 4x2.0 GHz Cortex-A720)",
            Gpu = "Immortalis-G720 MC12",
            DahiliHafiza = "256GB 12GB RAM, 512GB 12GB RAM, 512GB 16GB RAM, 1TB 12GB RAM",
            AnaKameraModulleri = "50 MP, f/1.6, 23mm (wide), 1/1.31\", 1.2\u00b5m, PDAF, OIS\n50 MP, f/2.0, 60mm (telephoto), 1/2.88\", 0.61\u00b5m, PDAF, 2.6x optical zoom\n12 MP, f/2.2, 15mm (ultrawide), 1/3.06\", 1.12\u00b5m",
            BataryaTipi = "Li-Po 5000 mAh",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "120W wired, PD3.0, QC4, 100% in 19 min\n50W wireless, 100% in 45 min",
            Fiyat = "$\u2009480.00 / \u00a3\u2009455.00 / \u20ac\u2009476.89",
            Url = "https://www.gsmarena.com/xiaomi_14t_pro-13328.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Xiaomi Redmi Pad 2",
            Marka = "Xiaomi",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/xiaomi-redmi-pad-2.jpg",
            NetworkTeknolojisi = "GSM / HSPA / LTE",
            DuyurulmaTarihi = "2025, June 05",
            PiyasayaCikisDurumu = "Available. Released 2025, June 05",
            EkranTipi = "IPS LCD, 1B colors, 90Hz, 600 nits (HBM)",
            EkranBoyutu = "11.0 inches, 350.9 cm2 (~83.0% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "Android 15, HyperOS 2",
            YongaSeti = "Mediatek Helio G100 Ultra (6 nm)",
            Cpu = "Octa-core (2x2.2 GHz Cortex-A76 & 6x2.0 GHz Cortex-A55)",
            Gpu = "Mali-G57 MC2",
            DahiliHafiza = "128GB 4GB RAM, 128GB 6GB RAM, 256GB 8GB RAM",
            AnaKameraModulleri = "8 MP, f/2.0, (wide), 1/4.0\", 1.12\u00b5m",
            BataryaTipi = "Li-Po 9000 mAh",
            BataryaDiger = null,
            SarjOzellikleri = "18W wired, PD2, QC2.0",
            Fiyat = "About 230 EUR",
            Url = "https://www.gsmarena.com/xiaomi_redmi_pad_2-13908.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Apple iPad (2022)",
            Marka = "Apple",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/apple-ipad-10-2022.jpg",
            NetworkTeknolojisi = "GSM / HSPA / LTE / 5G",
            DuyurulmaTarihi = "2022, October 18",
            PiyasayaCikisDurumu = "Available. Released 2022, October 26",
            EkranTipi = "Liquid Retina IPS LCD, 500 nits (typ)",
            EkranBoyutu = "10.9 inches, 359.2 cm2 (~80.5% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "iPadOS 16.1, upgradable to iPadOS 18.5",
            YongaSeti = "Apple A14 Bionic (5 nm)",
            Cpu = "Hexa-core (2x3.0 GHz Firestorm + 4x1.8 GHz Icestorm)",
            Gpu = "Apple GPU (4-core graphics)",
            DahiliHafiza = "64GB 4GB RAM, 256GB 4GB RAM",
            AnaKameraModulleri = "12 MP, f/1.8, (wide), PDAF",
            BataryaTipi = "Li-Po 7606 mAh (28.6 Wh)",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "Bilgi Yok",
            Fiyat = "About 430 EUR",
            Url = "https://www.gsmarena.com/apple_ipad_(2022)-11941.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Apple iPad (2025)",
            Marka = "Apple",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/apple-ipad-11-inch-2025.jpg",
            NetworkTeknolojisi = "GSM / HSPA / LTE / 5G",
            DuyurulmaTarihi = "2025, March 04",
            PiyasayaCikisDurumu = "Available. Released 2025, March 12",
            EkranTipi = "Liquid Retina IPS LCD, 500 nits (typ)",
            EkranBoyutu = "11.0 inches, 357.6 cm2 (~80.1% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "iPadOS 18.3.2, upgradable to iPadOS 18.5",
            YongaSeti = "Apple A16 Bionic (4 nm)",
            Cpu = "5-core",
            Gpu = "Apple GPU (4-core graphics)",
            DahiliHafiza = "128GB 6GB RAM, 256GB 6GB RAM, 512GB 6GB RAM",
            AnaKameraModulleri = "12 MP, f/1.8, (wide), PDAF",
            BataryaTipi = "Li-Po (28.93 Wh)",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "Bilgi Yok",
            Fiyat = "About 400 EUR",
            Url = "https://www.gsmarena.com/apple_ipad_(2025)-13702.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Apple iPad 10.2 (2019)",
            Marka = "Apple",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/apple-ipad7-102-inches.jpg",
            NetworkTeknolojisi = "GSM / CDMA / HSPA / EVDO / LTE",
            DuyurulmaTarihi = "2019, September 12",
            PiyasayaCikisDurumu = "Available. Released 2019, September 21",
            EkranTipi = "IPS LCD",
            EkranBoyutu = "10.2 inches, 324.6 cm2 (~74.4% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "iOS 13, upgradable to iPadOS 18.5",
            YongaSeti = "Apple A10 Fusion (16 nm)",
            Cpu = "Quad-core 2.34 GHz (2x Hurricane + 2x Zephyr)",
            Gpu = "PowerVR Series7XT Plus (six-core graphics)",
            DahiliHafiza = "32GB 3GB RAM, 128GB 3GB RAM",
            AnaKameraModulleri = "8 MP, f/2.4, 31mm (standard), 1.12\u00b5m, AF",
            BataryaTipi = "Li-Po 8827 mAh, non-removable (32.9 Wh)",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "Bilgi Yok",
            Fiyat = "About 350 EUR",
            Url = "https://www.gsmarena.com/apple_ipad_10_2_(2019)-9857.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Apple iPad 10.2 (2020)",
            Marka = "Apple",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/apple-ipad8-102-inches-2020.jpg",
            NetworkTeknolojisi = "GSM / HSPA / LTE",
            DuyurulmaTarihi = "2020, September 15",
            PiyasayaCikisDurumu = "Available. Released 2020, September 18",
            EkranTipi = "Retina IPS LCD, 500 nits (typ)",
            EkranBoyutu = "10.2 inches, 324.6 cm2 (~74.4% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "iPadOS 14, upgradable to iPadOS 18.5",
            YongaSeti = "Apple A12 Bionic (7 nm)",
            Cpu = "Hexa-core (2x2.5 GHz Vortex + 4x1.6 GHz Tempest)",
            Gpu = "Apple GPU (4-core graphics)",
            DahiliHafiza = "32GB 3GB RAM, 128GB 3GB RAM",
            AnaKameraModulleri = "8 MP, f/2.4, 31mm (standard), 1.12\u00b5m, AF",
            BataryaTipi = "Li-Ion 8827 mAh (32.4 Wh)",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "Bilgi Yok",
            Fiyat = "About 370 EUR",
            Url = "https://www.gsmarena.com/apple_ipad_10_2_(2020)-10445.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Apple iPad 10.2 (2021)",
            Marka = "Apple",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/apple-ipad-102-2021-.jpg",
            NetworkTeknolojisi = "GSM / HSPA / LTE",
            DuyurulmaTarihi = "2021, September 14",
            PiyasayaCikisDurumu = "Available. Released 2021, September 24",
            EkranTipi = "Retina IPS LCD, 500 nits (typ)",
            EkranBoyutu = "10.2 inches, 322.2 cm2 (~73.8% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "iPadOS 15, upgradable to iPadOS 18.5",
            YongaSeti = "Apple A13 Bionic (7 nm+)",
            Cpu = "Hexa-core (2x2.65 GHz Lightning + 4x1.8 GHz Thunder)",
            Gpu = "Apple GPU (4-core graphics)",
            DahiliHafiza = "64GB 3GB RAM, 256GB 3GB RAM",
            AnaKameraModulleri = "8 MP, f/2.4, 31mm (standard), 1.12\u00b5m, AF",
            BataryaTipi = "Li-Ion 8557 mAh (32.4 Wh)",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "Bilgi Yok",
            Fiyat = "About 430 EUR",
            Url = "https://www.gsmarena.com/apple_ipad_10_2_(2021)-11106.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Apple iPad 2 CDMA",
            Marka = "Apple",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/apple-ipad2-new.jpg",
            NetworkTeknolojisi = "CDMA / EVDO",
            DuyurulmaTarihi = "2011, March. Released 2011, March",
            PiyasayaCikisDurumu = "Discontinued",
            EkranTipi = "IPS LCD",
            EkranBoyutu = "9.7 inches, 291.4 cm2 (~65.1% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "iOS 4, upgradable to iOS 9.2.1",
            YongaSeti = "Apple A5 (45 nm)",
            Cpu = "Dual-core 1.0 GHz Cortex-A9",
            Gpu = "PowerVR SGX543MP2",
            DahiliHafiza = "16GB 512MB RAM, 32GB 512MB RAM, 64GB 512MB RAM",
            AnaKameraModulleri = "0.7 MP",
            BataryaTipi = "Non-removable Li-Po 6930 mAh battery (25 Wh)",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "Bilgi Yok",
            Fiyat = "About 200 EUR",
            Url = "https://www.gsmarena.com/apple_ipad_2_cdma-3849.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Apple iPad 2 Wi-Fi",
            Marka = "Apple",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/apple-ipad2-new.jpg",
            NetworkTeknolojisi = "No cellular connectivity",
            DuyurulmaTarihi = "2011, March. Released 2011, March",
            PiyasayaCikisDurumu = "Discontinued",
            EkranTipi = "IPS LCD",
            EkranBoyutu = "9.7 inches, 291.4 cm2 (~65.1% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "iOS 4, upgradable to iOS 9.3.5",
            YongaSeti = "Apple A5 (45 nm)",
            Cpu = "Dual-core 1.0 GHz Cortex-A9",
            Gpu = "PowerVR SGX543MP2",
            DahiliHafiza = "16GB 512MB RAM, 32GB 512MB RAM, 64GB 512MB RAM",
            AnaKameraModulleri = "0.7 MP",
            BataryaTipi = "Non-removable Li-Po 6930 mAh battery (25 Wh)",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "Bilgi Yok",
            Fiyat = "About 200 EUR",
            Url = "https://www.gsmarena.com/apple_ipad_2_wi_fi-3847.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Apple iPad 2 Wi-Fi + 3G",
            Marka = "Apple",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/apple-ipad2-new.jpg",
            NetworkTeknolojisi = "GSM / HSPA",
            DuyurulmaTarihi = "2011, March. Released 2011, March",
            PiyasayaCikisDurumu = "Discontinued",
            EkranTipi = "IPS LCD",
            EkranBoyutu = "9.7 inches, 291.4 cm2 (~65.1% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "iOS 4, upgradable to iOS 9.3.5",
            YongaSeti = "Apple A5 (45 nm)",
            Cpu = "Dual-core 1.0 GHz Cortex-A9",
            Gpu = "PowerVR SGX543MP2",
            DahiliHafiza = "16GB 512MB RAM, 32GB 512MB RAM, 64GB 512MB RAM",
            AnaKameraModulleri = "0.7 MP",
            BataryaTipi = "Non-removable Li-Po 6930 mAh battery (25 Wh)",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "Bilgi Yok",
            Fiyat = "About 370 EUR",
            Url = "https://www.gsmarena.com/apple_ipad_2_wi_fi_+_3g-3848.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Apple iPad 4 Wi-Fi + Cellular",
            Marka = "Apple",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/apple-ipad-3-new.jpg",
            NetworkTeknolojisi = "GSM / CDMA / HSPA / EVDO / LTE",
            DuyurulmaTarihi = "2012, October. Released 2012, November",
            PiyasayaCikisDurumu = "Discontinued",
            EkranTipi = "IPS LCD",
            EkranBoyutu = "9.7 inches, 291.4 cm2 (~65.1% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "iOS 6, upgradable to iOS 10.3",
            YongaSeti = "Apple A6X (32 nm)",
            Cpu = "Dual-core 1.4 GHz",
            Gpu = "PowerVR SGX554MP4 (quad-core graphics)",
            DahiliHafiza = "16GB 1GB RAM, 32GB 1GB RAM, 64GB 1GB RAM, 128GB 1GB RAM",
            AnaKameraModulleri = "5 MP, AF",
            BataryaTipi = "Li-Po 11560 mAh, non-removable (42.5 Wh)",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "Bilgi Yok",
            Fiyat = "About 500 EUR",
            Url = "https://www.gsmarena.com/apple_ipad_4_wi_fi_+_cellular-5071.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Apple iPad 9.7 (2017)",
            Marka = "Apple",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/apple-ipad-97-2017.jpg",
            NetworkTeknolojisi = "GSM / CDMA / HSPA / EVDO / LTE",
            DuyurulmaTarihi = "2017, March. Released 2017, March",
            PiyasayaCikisDurumu = "Discontinued",
            EkranTipi = "IPS LCD",
            EkranBoyutu = "9.7 inches, 291.4 cm2 (~71.6% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "iOS 10.3, upgradable to iPadOS 16.7.7",
            YongaSeti = "Apple A9 (14 nm)",
            Cpu = "Dual-core 1.84 GHz (Twister)",
            Gpu = "PowerVR GT7600 (six-core graphics)",
            DahiliHafiza = "32GB 2GB RAM, 128GB 2GB RAM",
            AnaKameraModulleri = "8 MP, f/2.4, 31mm (standard), 1.12\u00b5m, AF",
            BataryaTipi = "Li-Po 8827 mAh, non-removable (32.9 Wh)",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "Bilgi Yok",
            Fiyat = "About 390 EUR",
            Url = "https://www.gsmarena.com/apple_ipad_9_7_(2017)-8620.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Apple iPad 9.7 (2018)",
            Marka = "Apple",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/apple-ipad-97-2018.jpg",
            NetworkTeknolojisi = "GSM / CDMA / HSPA / EVDO / LTE",
            DuyurulmaTarihi = "2018, March 27. Released 2018, March 27",
            PiyasayaCikisDurumu = "Discontinued",
            EkranTipi = "IPS LCD",
            EkranBoyutu = "9.7 inches, 291.4 cm2 (~71.6% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "iOS 11.3, upgradable to iPadOS 17.6.1",
            YongaSeti = "Apple A10 Fusion (16 nm)",
            Cpu = "Quad-core 2.34 GHz (2x Hurricane + 2x Zephyr)",
            Gpu = "PowerVR Series7XT Plus (six-core graphics)",
            DahiliHafiza = "32GB 2GB RAM, 128GB 2GB RAM",
            AnaKameraModulleri = "8 MP, f/2.4, 31mm (standard), 1.12\u00b5m, AF",
            BataryaTipi = "Li-Po 8827 mAh, non-removable (32.9 Wh)",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "Bilgi Yok",
            Fiyat = "About 350 EUR",
            Url = "https://www.gsmarena.com/apple_ipad_9_7_(2018)-9142.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Apple iPad Air",
            Marka = "Apple",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/apple-ipad-air.jpg",
            NetworkTeknolojisi = "GSM / CDMA / HSPA / EVDO / LTE",
            DuyurulmaTarihi = "2013, October 22. Released 2013, November 01",
            PiyasayaCikisDurumu = "Discontinued",
            EkranTipi = "IPS LCD",
            EkranBoyutu = "9.7 inches, 291.4 cm2 (~71.6% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "iOS 7, upgradable to iPadOS 12.5.6",
            YongaSeti = "Apple A7 (28 nm)",
            Cpu = "Dual-core 1.3 GHz Cyclone (ARM v8-based)",
            Gpu = "PowerVR G6430 (quad-core graphics)",
            DahiliHafiza = "16GB 1GB RAM, 32GB 1GB RAM, 64GB 1GB RAM, 128GB 1GB RAM",
            AnaKameraModulleri = "5 MP, f/2.4, 33mm (standard), AF",
            BataryaTipi = "Li-Po 8600 mAh, non-removable (32.4 Wh)",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "Bilgi Yok",
            Fiyat = "About 350 EUR",
            Url = "https://www.gsmarena.com/apple_ipad_air-5797.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Apple iPad Air (2019)",
            Marka = "Apple",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/apple-ipad-air3-2019.jpg",
            NetworkTeknolojisi = "GSM / HSPA / LTE",
            DuyurulmaTarihi = "2019, March 18",
            PiyasayaCikisDurumu = "Available. Released 2019, March 18",
            EkranTipi = "IPS LCD",
            EkranBoyutu = "10.5 inches, 341.4 cm2 (~78.3% screen-to-body ratio)",
            EkranDigerOzellikler = "True-tone",
            IsletimSistemi = "iOS 12.1.3, upgradable to iPadOS 18.5",
            YongaSeti = "Apple A12 Bionic (7 nm)",
            Cpu = "Hexa-core (2x2.5 GHz Vortex + 4x1.6 GHz Tempest)",
            Gpu = "Apple GPU (4-core graphics)",
            DahiliHafiza = "64GB 3GB RAM, 256GB 3GB RAM",
            AnaKameraModulleri = "8 MP, f/2.4, 31mm (standard), 1.12\u00b5m, AF",
            BataryaTipi = "Li-Po 8134 mAh, non-removable (30.8 Wh)",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "Bilgi Yok",
            Fiyat = "About 550 EUR",
            Url = "https://www.gsmarena.com/apple_ipad_air_(2019)-9638.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Apple iPad Air (2020)",
            Marka = "Apple",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/apple-ipad-air4-2020.jpg",
            NetworkTeknolojisi = "GSM / HSPA / LTE",
            DuyurulmaTarihi = "2020, September 15",
            PiyasayaCikisDurumu = "Available. Released 2020, October 23",
            EkranTipi = "Liquid Retina IPS LCD, 500 nits (typ)",
            EkranBoyutu = "10.9 inches, 359.2 cm2 (~81.3% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "iPadOS 14.1, upgradable to iPadOS 18.5",
            YongaSeti = "Apple A14 Bionic (5 nm)",
            Cpu = "Hexa-core (2x3.0 GHz Firestorm + 4x1.8 GHz Icestorm)",
            Gpu = "Apple GPU (4-core graphics)",
            DahiliHafiza = "64GB 4GB RAM, 256GB 4GB RAM",
            AnaKameraModulleri = "12 MP, f/1.8, (wide), 1/3.0\", 1.22\u00b5m, dual pixel PDAF",
            BataryaTipi = "Li-Ion 7606 mAh (28.93 Wh)",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "Bilgi Yok",
            Fiyat = "About 640 EUR",
            Url = "https://www.gsmarena.com/apple_ipad_air_(2020)-10444.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Apple iPad Air (2022)",
            Marka = "Apple",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/apple-ipad-air-2022-new.jpg",
            NetworkTeknolojisi = "GSM / HSPA / LTE / 5G",
            DuyurulmaTarihi = "2022, March 08",
            PiyasayaCikisDurumu = "Available. Released 2022, March 18",
            EkranTipi = "Liquid Retina IPS LCD, 500 nits (typ)",
            EkranBoyutu = "10.9 inches, 359.2 cm2 (~81.3% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "iPadOS 15.4, upgradable to iPadOS 18.5",
            YongaSeti = "Apple M1",
            Cpu = "Octa-core (4x3.2 GHz & 4xX.X GHz)",
            Gpu = "Apple GPU (8-core graphics)",
            DahiliHafiza = "64GB, 256GB 8GB RAM",
            AnaKameraModulleri = "12 MP, f/1.8, (wide), 1/3.0\", 1.22\u00b5m, dual pixel PDAF",
            BataryaTipi = "Li-Ion 7600 mAh (28.6 Wh)",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "Bilgi Yok",
            Fiyat = "About 550 EUR",
            Url = "https://www.gsmarena.com/apple_ipad_air_(2022)-11411.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Apple iPad Air 11 (2024)",
            Marka = "Apple",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/apple-ipad-air-11-2024.jpg",
            NetworkTeknolojisi = "GSM / HSPA / LTE / 5G",
            DuyurulmaTarihi = "2024, May 07",
            PiyasayaCikisDurumu = "Available. Released 2024, May 15",
            EkranTipi = "Liquid Retina IPS LCD, 500 nits (typ)",
            EkranBoyutu = "11.0 inches, 357.6 cm2 (~80.9% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "iPadOS 17.4, upgradable to iPadOS 18.5",
            YongaSeti = "Apple M2",
            Cpu = "Octa-core (4x3.48 GHz performance cores and 4 efficiency cores)",
            Gpu = "Apple GPU (9-core graphics)",
            DahiliHafiza = "128GB 8GB RAM, 256GB 8GB RAM, 512GB 8GB RAM, 1TB 8GB RAM",
            AnaKameraModulleri = "12 MP, f/1.8, (wide), 1/3.0\", 1.22\u00b5m, dual pixel PDAF",
            BataryaTipi = "Li-Po 7606 mAh (28.93 Wh)",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "45W wired",
            Fiyat = "About 700 EUR",
            Url = "https://www.gsmarena.com/apple_ipad_air_11_(2024)-12984.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Apple iPad Air 11 (2025)",
            Marka = "Apple",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/apple-ipad-air-11-2025.jpg",
            NetworkTeknolojisi = "GSM / HSPA / LTE / 5G",
            DuyurulmaTarihi = "2025, March 04",
            PiyasayaCikisDurumu = "Available. Released 2025, March 12",
            EkranTipi = "Liquid Retina IPS LCD, 500 nits (typ)",
            EkranBoyutu = "11.0 inches, 357.6 cm2 (~80.9% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "iPadOS 18.3.2, upgradable to iPadOS 18.5",
            YongaSeti = "Apple M3",
            Cpu = "Octa-core (4 performance cores and 4 efficiency cores)",
            Gpu = "Apple GPU (9-core graphics)",
            DahiliHafiza = "128GB 8GB RAM, 256GB 8GB RAM, 512GB 8GB RAM, 1TB 8GB RAM",
            AnaKameraModulleri = "12 MP, f/1.8, (wide), 1/3.0\", 1.22\u00b5m, dual pixel PDAF",
            BataryaTipi = "Li-Po 7606 mAh (28.93 Wh)",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "Bilgi Yok",
            Fiyat = "About 700 EUR",
            Url = "https://www.gsmarena.com/apple_ipad_air_11_(2025)-13703.php",
        },
        new SeedTelefonRow
        {
            ModelAdi = "Apple iPad Air 13 (2024)",
            Marka = "Apple",
            ResimUrl = "https://fdn2.gsmarena.com/vv/bigpic/apple-ipad-air-13-2024.jpg",
            NetworkTeknolojisi = "GSM / HSPA / LTE / 5G",
            DuyurulmaTarihi = "2024, May 07",
            PiyasayaCikisDurumu = "Available. Released 2024, May 15",
            EkranTipi = "Liquid Retina IPS LCD, 600 nits (typ)",
            EkranBoyutu = "13.0 inches, 519.3 cm2 (~86.1% screen-to-body ratio)",
            EkranDigerOzellikler = "Bilgi Yok",
            IsletimSistemi = "iPadOS 17.5.1, upgradable to iPadOS 18.5",
            YongaSeti = "Apple M2",
            Cpu = "Octa-core (4x3.46 GHz performance cores and 4 efficiency cores)",
            Gpu = "Apple GPU (9-core graphics)",
            DahiliHafiza = "128GB 8GB RAM, 256GB 8GB RAM, 512GB 8GB RAM, 1TB 8GB RAM",
            AnaKameraModulleri = "12 MP, f/1.8, (wide), 1/3.0\", 1.22\u00b5m, dual pixel PDAF",
            BataryaTipi = "Li-Po 9705 mAh (36.59 Wh)",
            BataryaDiger = "Bilgi Yok",
            SarjOzellikleri = "45W wired",
            Fiyat = "About 950 EUR",
            Url = "https://www.gsmarena.com/apple_ipad_air_13_(2024)-12985.php",
        },
    ];

    private readonly KiyaslasanaDbContext _dbContext;

    public InitialTelefonSeeder(KiyaslasanaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _dbContext.Telefonlar.AnyAsync(ct))
        {
            return;
        }

        var usedSlugs = new HashSet<string>(StringComparer.Ordinal);
        var phones = new List<Telefon>(SeedRows.Count);

        foreach (var row in SeedRows)
        {
            phones.Add(new Telefon
            {
                ModelAdi = row.ModelAdi,
                Marka = row.Marka,
                ResimUrl = row.ResimUrl,
                NetworkTeknolojisi = row.NetworkTeknolojisi,
                DuyurulmaTarihi = row.DuyurulmaTarihi,
                PiyasayaCikisDurumu = row.PiyasayaCikisDurumu,
                EkranTipi = row.EkranTipi,
                EkranBoyutu = row.EkranBoyutu,
                EkranDigerOzellikler = row.EkranDigerOzellikler,
                IsletimSistemi = row.IsletimSistemi,
                YongaSeti = row.YongaSeti,
                Cpu = row.Cpu,
                Gpu = row.Gpu,
                DahiliHafiza = row.DahiliHafiza,
                AnaKameraModulleri = row.AnaKameraModulleri,
                BataryaTipi = row.BataryaTipi,
                BataryaDiger = row.BataryaDiger,
                SarjOzellikleri = row.SarjOzellikleri,
                Fiyat = row.Fiyat,
                Url = row.Url,
                Slug = BuildUniqueSlug(row.Marka, row.ModelAdi, usedSlugs)
            });
        }

        await _dbContext.Telefonlar.AddRangeAsync(phones, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    private static string BuildUniqueSlug(string? marka, string? modelAdi, HashSet<string> usedSlugs)
    {
        var rawBase = $"{marka ?? string.Empty}-{modelAdi ?? string.Empty}"
            .ToLowerInvariant()
            .Replace(" ", "-");

        var baseSlug = NormalizeSlug(rawBase);
        var candidate = baseSlug;
        var suffix = 2;

        while (!usedSlugs.Add(candidate))
        {
            var suffixPart = "-" + suffix;
            var maxBaseLength = TelefonConstraints.SlugMaxLength - suffixPart.Length;
            var trimmedBase = baseSlug.Length <= maxBaseLength
                ? baseSlug
                : baseSlug[..maxBaseLength].Trim('-');

            if (trimmedBase.Length == 0)
            {
                trimmedBase = "telefon";
            }

            candidate = trimmedBase + suffixPart;
            suffix++;
        }

        return candidate;
    }

    private static string NormalizeSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "telefon";
        }

        var builder = new StringBuilder(value.Length);
        var previousDash = false;

        foreach (var rawCh in value)
        {
            var ch = char.ToLowerInvariant(rawCh);
            if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
            {
                builder.Append(ch);
                previousDash = false;
                continue;
            }

            if (previousDash)
            {
                continue;
            }

            builder.Append('-');
            previousDash = true;
        }

        var slug = builder.ToString().Trim('-');
        if (slug.Length > TelefonConstraints.SlugMaxLength)
        {
            slug = slug[..TelefonConstraints.SlugMaxLength].Trim('-');
        }

        return slug.Length == 0 ? "telefon" : slug;
    }

    private sealed class SeedTelefonRow
    {
        public string? ModelAdi { get; init; }
        public string? Marka { get; init; }
        public string? ResimUrl { get; init; }
        public string? NetworkTeknolojisi { get; init; }
        public string? DuyurulmaTarihi { get; init; }
        public string? PiyasayaCikisDurumu { get; init; }
        public string? EkranTipi { get; init; }
        public string? EkranBoyutu { get; init; }
        public string? EkranDigerOzellikler { get; init; }
        public string? IsletimSistemi { get; init; }
        public string? YongaSeti { get; init; }
        public string? Cpu { get; init; }
        public string? Gpu { get; init; }
        public string? DahiliHafiza { get; init; }
        public string? AnaKameraModulleri { get; init; }
        public string? BataryaTipi { get; init; }
        public string? BataryaDiger { get; init; }
        public string? SarjOzellikleri { get; init; }
        public string? Fiyat { get; init; }
        public string? Url { get; init; }
    }
}
