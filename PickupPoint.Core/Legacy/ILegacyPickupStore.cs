namespace PickupPoint.Core.Legacy;

public interface ILegacyPickupStore
{
    void Add(LegacyPickupOrder order);

    LegacyPickupOrder GetById(int id);

    void SaveToFile(LegacyPickupOrder order, string path);

    void SendEmail(string to, string body);

    string ExportCsv();

    void Backup(string folder);

    void PrintInvoice(LegacyPickupOrder order);
}
