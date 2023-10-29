// <copyright file="FileTypes.cs" company="Rocket Robin">
// Copyright (c) Rocket Robin. All rights reserved.
// Licensed under the Apache v2 license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

namespace Courses.API.Helper.FileFormatValidation
{
    /// <summary>
    /// Common file types for populate a new sniffer instance.
    /// </summary>

    [Obsolete("please use populate the file types only you need.")]
    public class FileTypes
    {
        static FileTypes()
        {
            
            Unfrequent = new List<FileRecord>
            {
                new FileRecord("bin", "53 50 30 31"),
                new FileRecord("bac", "42 41 43 4B 4D 49 4B 45 44 49 53 4B"),
                new FileRecord("bz2", "42 5A 68"),
                new FileRecord("tif tiff", "49 49 2A 00"),
                new FileRecord("tif tiff", "4D 4D 00 2A"),
                new FileRecord("cr2", "49 49 2A 00 10 00 00 00 43 52"),
                new FileRecord("cin", "80 2A 5F D7"),
                new FileRecord("exr", "76 2F 31 01"),
                new FileRecord("dpx", "53 44 50 58"),
                new FileRecord("dpx", "58 50 44 53"),
                new FileRecord("bpg", "42 50 47 FB"),
                new FileRecord("lz", "4C 5A 49 50"),
                new FileRecord("ps", "25 21 50 53"),
                new FileRecord("fits", "3D 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 20 54"),
                new FileRecord("doc xls ppt msg", "D0 CF 11 E0 A1 B1 1A E1"),
                new FileRecord("dex", "64 65 78 0A 30 33 35 00"),
                new FileRecord("vmdk", "4B 44 4D"),
                new FileRecord("crx", "43 72 32 34"),
                new FileRecord("cwk", "05 07 00 00 42 4F 42 4F 05 07 00 00 00 00 00 00 00 00 00 00 00 01"),
                new FileRecord("fh8", "41 47 44 33"),
                new FileRecord("cwk", "06 07 E1 00 42 4F 42 4F 06 07 E1 00 00 00 00 00 00 00 00 00 00 01"),
                new FileRecord("toast", "45 52 02 00 00 00"),
                new FileRecord("toast", "8B 45 52 02 00 00 00"),
                new FileRecord("xar", "78 61 72 21"),
                new FileRecord("dat", "50 4D 4F 43 43 4D 4F 43"),
                new FileRecord("nes", "4E 45 53 1A"),
                new FileRecord("tox", "74 6F 78 33"),
                new FileRecord("MLV", "4D 4C 56 49"),
                new FileRecord("lz4", "04 22 4D 18"),
                new FileRecord("cab", "4D 53 43 46"),
                new FileRecord("flif", "46 4C 49 46"),
                new FileRecord("stg", "4D 49 4C 20"),
                new FileRecord("der", "30 82"),
                new FileRecord("wasm", "00 61 73 6d"),
                new FileRecord("lep", "cf 84 01"),
                new FileRecord("rtf", "7B 5C 72 74 66 31"),
                new FileRecord("m2p vob", "00 00 01 BA"),
                new FileRecord("zlib", "78 01"),
                new FileRecord("zlib", "78 9c"),
                new FileRecord("zlib", "78 da"),
                new FileRecord("lzfse", "62 76 78 32"),
                new FileRecord("orc", "4F 52 43"),
                new FileRecord("avro", "4F 62 6A 01"),
                new FileRecord("rc", "53 45 51 36"),
                new FileRecord("tbi", "00 00 00 00 14 00 00 00"),
                new FileRecord("dat", "00 00 00 00 62 31 05 00 09 00 00 00 00 20 00 00 00 09 00 00 00 00 00 00", 8, "Bitcoin Core wallet.dat file"),
                new FileRecord("jp2", "00 00 00 0C 6A 50 20 20 0D 0A", "Various JPEG-2000 image file formats"),
                new FileRecord("ttf", "00 01 00 00 00"),
                new FileRecord("mdf", "00 FF FF FF FF FF FF FF FF FF FF 00 00 02 00 01"),

                // Complex file type.
                new FileRecord("pdb", "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00", 11),
                new FileRecord("3gp 3g2", "66 74 79 70 33 67", 4),
                new FileRecord("iso", "43 44 30 30 31", 32769),
                new FileRecord("iso", "43 44 30 30 31", 34817),
                new FileRecord("iso", "43 44 30 30 31", 36865),
            };
            Common = new List<FileRecord>
            {
                new FileRecord("asf wma wmv", "30 26 B2 75 8E 66 CF 11 A6 D9 00 AA 00 62 CE 6C"),
                new FileRecord("ogg oga ogv", "4F 67 67 53"),
                new FileRecord("psd", "38 42 50 53"),
                new FileRecord("mp3", "FF FB"),
                new FileRecord("mp3", "49 44 33"),
                new FileRecord("bmp dib", "42 4D"),
                new FileRecord("jpg,jpeg", "ff,d8,ff,db"),
                new FileRecord("png", "89,50,4e,47,0d,0a,1a,0a"),
                new FileRecord("zip,jar,odt,ods,odp,docx,xlsx,pptx,vsdx,apk,aar", "50,4b,03,04"),
                new FileRecord("zip,jar,odt,ods,odp,docx,xlsx,pptx,vsdx,apk,aar", "50,4b,07,08"),
                new FileRecord("zip,jar,odt,ods,odp,docx,xlsx,pptx,vsdx,apk,aar", "50,4b,05,06"),
                new FileRecord("rar", "52,61,72,21,1a,07,00"),
                new FileRecord("rar", "52,61,72,21,1a,07,01,00"),
                new FileRecord("class", "CA FE BA BE"),
                new FileRecord("pdf", "25 50 44 46"),
                new FileRecord("rpm", "ed ab ee db"),
                new FileRecord("flac", "66 4C 61 43"),
                new FileRecord("mid midi", "4D 54 68 64"),
                new FileRecord("ico", "00 00 01 00"),
                new FileRecord("z,tar.z", "1F 9D"),
                new FileRecord("z,tar.z", "1F A0"),
                new FileRecord("gif", "47 49 46 38 37 61"),
                new FileRecord("dmg", "78 01 73 0D 62 62 60"),
                new FileRecord("gif", "47 49 46 38 39 61"),
                new FileRecord("exe", "4D 5A"),
                new FileRecord("tar", "75 73 74 61 72", 257),
                new FileRecord("mkv mka mks mk3d webm", "1A 45 DF A3"),
                new FileRecord("gz tar.gz", "1F 8B"),
                new FileRecord("xz tar.xz", "FD 37 7A 58 5A 00 00"),
                new FileRecord("7z", "37 7A BC AF 27 1C"),
                new FileRecord("mpg mpeg", "00 00 01 BA"),
                new FileRecord("mpg mpeg", "00 00 01 B3"),
                new FileRecord("woff", "77 4F 46 46"),
                new FileRecord("woff2", "77 4F 46 32"),
                new FileRecord("XML", "3c 3f 78 6d 6c 20"),
                new FileRecord("swf", "43 57 53"),
                new FileRecord("swf", "46 57 53"),
                new FileRecord("deb", "21 3C 61 72 63 68 3E"),

                // complext
                new FileRecord("jpg,jpeg","FF D8 FF E0 ?? ?? 4A 46 49 46 00 01"),
                new FileRecord("jpg,jpeg","FF D8 FF E1 ?? ?? 45 78 69 66 00 00"),
            };


           

            Discussioforumn = new List<FileRecord>
            {
                        new FileRecord("doc xls ppt msg", "D0 CF 11 E0 A1 B1 1A E1"),
                        new FileRecord("jpg,jpeg", "ff,d8,ff,db"),
                        new FileRecord("png", "89,50,4e,47,0d,0a,1a,0a"),
                        new FileRecord("pdf", "25 50 44 46"),
                        new FileRecord("gif", "47 49 46 38 39 61"),
                        new FileRecord("ogg oga ogv", "4F 67 67 53"),
                        new FileRecord("mp3", "FF FB"),
                        new FileRecord("mp3", "49 44 33"),
                        new FileRecord("bmp dib", "42 4D"),
                        new FileRecord("mkv mka mks mk3d webm", "1A 45 DF A3"),
                        new FileRecord("mpg mpeg", "00 00 01 BA"),
                        new FileRecord("mpg mpeg", "00 00 01 B3"),
                        new FileRecord("mp4","66 74 79 70 69 73 6F 6D"),
                        new FileRecord("docx,xlsx,pptx", "50,4b,03,04"),
                new FileRecord("docx,xlsx,pptx", "50,4b,07,08"),
                new FileRecord("docx,xlsx,pptx", "50,4b,05,06"),
                new FileRecord ("wav","57 41 56 45"),
            };
        }

        /// <summary>
        /// Gets CommonFileTypes.
        /// Replace this with <see cref="Common"/>
        /// </summary>
        [Obsolete("please use populate the file types only you need.")]
        public static List<FileRecord> CommonFileTypes { get => Common; }

       // [Obsolete("please use populate the file types only you need.")]
        public static List<FileRecord> DiscussioforumnFileTypes { get => Discussioforumn; }

        /// <summary>
        /// Gets Common It contains the format of the file we often see.
        /// </summary>
        [Obsolete("please use populate the file types only you need.")]
        public static List<FileRecord> Common { get; set; }
        public static List<FileRecord> Discussioforumn { get; set; }


        /// <summary>
        /// It contains unfrequent file formats.
        /// </summary>
        [Obsolete("please use populate the file types only you need.")]
        public static List<FileRecord> Unfrequent { get; set; }
    }
}