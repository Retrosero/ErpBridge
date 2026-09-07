using System.Collections.Generic;

namespace Fora.Mikro.Interface;

public interface IForaPlatformTools
{
	bool Exists(string filename);

	bool Delete(string filename);

	bool DirectoryExists(string directoryname);

	void DirectoryCreate(string directoryname);

	string GetFileNameWithPath(string filename);

	string GetPath();

	string GetPicturePath();

	string GetDownloadPath();

	void CreateDb(string filename);

	List<string> GetKullaniciAdiSifre();

	void SetKullaniciAdiSifre(bool HatirlaKullaniciAdi, string KullaniciAdi, bool HatirlaSifre, string Sifre);

	bool NetworkKontrol();

	string GetDeviceID();

	string MakeMD5(string metin);

	string EncryptText(string key, string toEncrypt, bool useHashing);

	string DecryptText(string key, string EncryptedText, bool useHashing);

	string UrlEncode(string value);

	object ToObjectGZip(byte[] byteArray);

	void FileShare(string title, string message, string filePath, string Format);
}
