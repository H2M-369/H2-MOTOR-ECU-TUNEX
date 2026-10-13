using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace _3D_Chart_Graph
{
	// Lisensi H2 MOTOR ECU TUNEX v2.
	// Format file license.lic :  H2LIC2.<payload base64url>.<tanda tangan RSA-SHA256 base64url>
	// payload (UTF-8)         :  H2MOTOR|2|<HWID hash SHA-256>|<nama>|<no. hp>|<tanggal terbit yyyyMMdd>
	// Aplikasi hanya memegang KUNCI PUBLIK (verifikasi). Kunci privat hanya ada di keygen "H2 MOTOR ECU TUNEX.exe",
	// sehingga lisensi tidak bisa dipalsukan dari isi aplikasi ini.
	internal static class H2License
	{
		public const string Prefix = "H2LIC2";

		private const string PublicKeyXml = "<RSAKeyValue><Modulus>nPZKt8NfR6HBzP6HttOmtoVAkH+ClqoMWX8MzQrxccObXmAYj0fuECuYB/5LSc0MDp3Sh01niokH7ScSnDAFO6Dymq0rR0qyF02OsRlOX4AyLzIxHl4j27ms5qlE8hNuwPw5mILxoJkbzB3ZdsINYHNFb/UtXS+2hvax3v1mync2P0mt76BrgABzjGFpDRNeUUs7/5NNaBLHC5VssVV+MhoxH4n7IR3whyygYs5QrsoC3TKHFaMjopKUjp77/RUWHHvPtiwFSBToF0+Xhavs0dRj7CnTGUFwf+0QLMCGR4aS9q/+EAGgVfbrFltzYPAr8c3kPHo5TehdTTK05FLAfQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

		public static string HwidHash(string combined)
		{
			using (SHA256 sha = SHA256.Create())
			{
				byte[] h = sha.ComputeHash(Encoding.UTF8.GetBytes("H2MOTOR-ECU-TUNEX|v2|" + (combined ?? "").Trim()));
				return BitConverter.ToString(h).Replace("-", "");
			}
		}

		private static byte[] FromB64Url(string s)
		{
			s = s.Replace('-', '+').Replace('_', '/');
			switch (s.Length % 4)
			{
				case 2: s += "=="; break;
				case 3: s += "="; break;
			}
			return Convert.FromBase64String(s);
		}

		// Cari license.lic di folder aplikasi (.exe) dulu, baru folder kerja. Folder kerja bisa berubah
		// setelah dialog Open/Save dipakai, jadi path relatif saja tidak andal.
		public static string ResolvePath(string path)
		{
			try
			{
				string a = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
				if (File.Exists(a)) return a;
			}
			catch (Exception) { }
			return path;
		}

		// Alasan lisensi ditolak (untuk pesan ke pengguna). Kosong bila lisensi valid.
		public static string Diagnose(string[] lic, string myHash)
		{
			if (lic == null || lic.Length < 3) return "Data lisensi tidak terbaca.";
			if (lic[0] == "Not Found")
				return "File license.lic tidak ditemukan.\nLetakkan license.lic di folder yang sama dengan aplikasi:\n" + AppDomain.CurrentDomain.BaseDirectory;
			if (lic[0] == "Error")
				return "license.lic tidak valid (format lama / rusak / bukan dibuat oleh keygen H2 MOTOR ECU TUNEX terbaru).\nBuat ulang dengan keygen baru.";
			if (lic[0] != myHash)
				return "license.lic ini dibuat untuk komputer lain (HWID tidak cocok).\nSimpan HWID.dat dari komputer ini, lalu buat lisensi ulang.";
			return "";
		}

		// Hasil: [hwidHash, nama, noHp]. Bila file tidak ada / rusak / tanda tangan salah: nama dan noHp kosong.
		public static string[] Load(string path)
		{
			string[] bad = new string[3] { "Error", "", "" };
			try
			{
				path = ResolvePath(path);
				if (!File.Exists(path))
				{
					return new string[3] { "Not Found", "", "" };
				}
				string[] parts = File.ReadAllText(path).Trim().Split('.');
				if (parts.Length != 3 || parts[0] != Prefix)
				{
					return bad;
				}
				byte[] payload = FromB64Url(parts[1]);
				byte[] sig = FromB64Url(parts[2]);
				using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
				{
					rsa.FromXmlString(PublicKeyXml);
					if (!rsa.VerifyData(payload, new SHA256CryptoServiceProvider(), sig))
					{
						return bad;
					}
				}
				string[] f = Encoding.UTF8.GetString(payload).Split('|');
				if (f.Length != 6 || f[0] != "H2MOTOR" || f[1] != "2")
				{
					return bad;
				}
				return new string[3] { f[2], f[3], f[4] };
			}
			catch (Exception)
			{
				return bad;
			}
		}
	}
}
