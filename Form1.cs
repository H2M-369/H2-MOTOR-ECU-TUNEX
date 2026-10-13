#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Media;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Plot3D;

namespace _3D_Chart_Graph;

public partial class Form1 : Form
{
	private delegate void SetControlTextCallBack(Control control, string newText);

	private delegate void SetProgressBarPositionCallBack(ProgressBar progressBar, int position);

	private enum ECUTYPE
	{
		KEIHINOTHER,
		KEIHINH8C,
		KEIHINM32R,
		SHINDENGENV850,
		SHINDENGENRH850,
		SHINDENGENRH850B
	}

	private enum STATUS
	{
		SUCCESS,
		FAILED
	}

	private double InputValue;

	private bool IsConfirmed;

	private string teks = "⟶ ( H2 MOTOR ECU TUNEX ) ⟵  H2 SPECIALIST TUNING  ⟶  ";

	private int selectedStartAddress;

	private static int startAddress = 0;

	private static int startAddress2 = 0;

	private static int startAddress3 = 0;

	private int columnCount = 1;

	private int rowCount = 1;

	private int numberOfRows = 0;

	private int numberOfColumnss = 0;

	private static byte[] binBytes;

	private static byte[] binBytesORI;

	private ushort minValue;

	private ushort maxValue;

	private Stack<byte[]> undoStack = new Stack<byte[]>();

	private Stack<byte[]> redoStack = new Stack<byte[]>();

	private static int tpsh = 0;

	private static int rpmh = 0;

	private static int iaph = 0;

	private static int feul1 = 0;

	private static int feul2 = 0;

	private static int feul3 = 0;

	private static int iap = 0;

	private static int ig1 = 0;

	private static int ig2 = 0;

	private static int ig3 = 0;

	private static int ig4 = 0;

	private static int limit1 = 0;

	private static int limit2 = 0;

	private static int limit3 = 0;

	private static int limit4 = 0;

	private static int limit5 = 0;

	private static int limit6 = 0;

	private static int limit7 = 0;

	private float lm;

	private float kon;

	private static int o2 = 0;

	private static int o2c = 0;

	private static int IMMO = 0;

	private static int MODE0 = 0;

	private static int MODE1 = 0;

	private static int MODE2 = 0;

	private static int MODE3 = 0;

	private static int MODE4 = 0;

	private static byte o2mask = 0;

	private static byte o2cmask = 0;

	private static byte immomask = 0;

	private static int bass = 0;

	private static byte bassmask = 0;

	private static int iacv = 0;

	private static byte iacvmask = 0;

	private static int ect = 0;

	private static byte ectmask = 0;

	private static int iat = 0;

	private static byte iatmask = 0;

	private static int vss = 0;

	private static byte vssmask = 0;

	private static int mapp = 0;

	private static byte mappmask = 0;

	private string initialValue;

	private static int binSize;

	private static bool isUnknown = false;

	private static IntPtr ftHandle = IntPtr.Zero;

	private static bool endThread = false;

	private static bool isNoInterface = true;

	private static bool isConnected = false;

	private static bool isRecovery = false;

	private static bool isFastWrite;

	private static int startOffset = 0;

	private static int checksumOffset = 0;

	private static bool con = false;

	private static bool ukur = false;

	private static bool MAAP = false;

	private string id2;

	private string text = "-";

	private static readonly byte[] shBinSig = new byte[4] { 90, 90, 90, 90 };

	private static readonly byte[] shWKeysig = new byte[7] { 17, 18, 19, 20, 21, 21, 23 };

	private static readonly byte[] shV850Sig = new byte[5] { 83, 86, 56, 53, 48 };

	private static readonly byte[] shRh850Sig = new byte[5] { 83, 72, 56, 53, 48 };

	private static readonly byte[] shStart0 = new byte[5] { 165, 165, 165, 165, 0 };

	private static readonly byte[] khH8cSig = new byte[4] { 255, 255, 170, 170 };

	private static readonly byte[] khM32rSig = new byte[7] { 255, 255, 255, 170, 170, 170, 170 };

	private static readonly byte[] shRKeysSig = new byte[6] { 145, 145, 13, 0, 158, 141 };

	private static readonly byte[] khRh850Sig = new byte[7] { 255, 255, 255, 0, 0, 0, 0 };

	private static readonly byte[] khMpc560Sig = new byte[7] { 255, 255, 255, 255, 255, 255, 255 };

	private static readonly byte[] khSpc5604Sig = new byte[7];

	private static readonly byte[] khSH705XSig = new byte[7] { 255, 255, 255, 170, 170, 170, 170 };

	private static readonly string[] khPartCode = new string[97]
	{
		"VERZA 150 (K18G-941)", "VERZA 150 (K18-941)", "CB 150 VERZA (K18-961)", "MEGAPRO 150 (KYE-942)", "BEAT POP (K61-601)", "BEAT-SCOOPY 110 (K25A-901)", "BEAT-SCOOPY 110 (K25-901)", "BLADE-SUPRA X 125 (K41-N01)", "CBR 250R (KYJ-971)", "CBR 250R (KYJ-922)",
		"CBR 250R (KYJ-901)", "REVO 110 (K03-N32)", "REVO 110 (K58-N41)", "REVO 110 (K58R-N81)", "CRF250 RALLY (KZZL-L41)", "CRF250 RALLY (KZZ-L41)", "SCOOPY DONAT (K93-N01)", "SCOOPY DONAT (K93-M01)", "SCOOPY 110 NEW (K2F-N01)", "SCOOPY 110 NEW (K2F-N11)",
		"SCOOPY 110 NEW (K2F-N81)", "SCOOPY 110 NEW (K2F-N91)", "CBR 150R Facelift (K45-N42)", "CBR 150R Facelift (K45-N11)", "CBR 150R Facelift (K45-NL1)", "CBR 150R ASIA (K45-NA1)", "CBR 150R Facelift (K45-N01)", "CBR 150R (KPP-N02)", "CBR 150R (KPP-602)", "CB 150 X (K38-N01)",
		"CB 150R (K15-771)", "CB 150R CIP (K15-601)", "CB 150R (K15-601)", "CB 150R OLD (K15-903)", "CB 150R OLD (K15-921)", "CB 150R Thailand (K15-888)", "GENIO 110 (K0J-N11)", "GENIO 110 (K0J-N61)", "GENIO 110 (K0J-N01)", "GENIO 110 (K0J-N71)",
		"CRF 150L (K84-901)", "SUPRA GTR150 (K56-N11)", "SUPRA GTR150 (K56-NJ1)", "SUPRA X125 (K41K-NC1)", "SUPRA X125 (K41-NC1)", "SUPRA X125 HELM-IN (KYZ-712)", "SUPRA X125 HELM-IN (KYZ-713)", "VARIO 110 ACG (K46-N21)", "VARIO 110 ACG (K46-N31)", "VARIO 110 ACG (K46-NA1)",
		"VARIO 110 ACG (K46-NB1)", "SUPRA X ANF (KTM-781)", "SUPRA X ANF (KVL-N21)", "VERZA 150 38770-K18G-941", "SCOOPY 110 NEW NON ISS (2020) 38770-K2F-N01", "VARIO||BEAT 110 K46-N01", "SPACY 110 38770-KZL-C31", "SONIC-38770-K56-N01", "Zoomer X 38770-K20-901", "Zoomer X 38770-K20-T01",
		"SCOOPY 110 THAILAND 30400-K2F-T21", "SCOOPY 110 THAILAND 30400-K2F-T81", "SCOOPY 110 THAILAND 30400-K2F-T91", "SCOOPY 110 THAILAND 30400-K93-T01", "SCOOPY 110 THAILAND 30400-K93-T21", "Wave 110 THAILAND 38770-K58-TC1", "Wave 110 THAILAND 38770-K58-TC2", "Wave 110 THAILAND 38770-K58-T81", "Wave 110 THAILAND 38770-K2J-TH1", "Wave 110 THAILAND 38770-K2J-T01",
		"VERZA-38770-K18-902", "Wave 110 THAILAND 38770-K03-T61", "Wave 110 THAILAND 38770-K03-H01", "Wave 110 THAILAND 38770-KWW-643", "REVO 110 (K58-N81)", "WAVE 125i THAILAND 38770-K3F-T01", "WAVE 125i THAILAND 38770-K73-T32", "WAVE 125i THAILAND 38770-K73-TC1", "WAVE 125i THAILAND 38770-KYZ-T41", "FUTURE 125 THAILAND 38770-KYZ-T03",
		"Dream Super Cub 38770-K76-T62", "Click 110 38770-KVB-S52", "Spacy 110 38770-KZL-C01", "CT 125 38770-K2E-T11", "Grom|MSX 125 38770-K26-B02", "Grom|MSX 125 38770-K26-921", "Grom | MSX 125 38770-K26-911", "Grom | MSX 125 38770-K26-901", "Dream 110 38770-K1M-T01", "Dream 110 38770-K1M-T31",
		"XRM 125 38770-CAP-H01", "RS 125 38770-KPG-H01", "RS 125 38770-KPG-H21", "XRM 125 38770-KPY-H01", "XRM 125 38770-KPY-H61", "Scoopy 110 30400-K2F-NC1", "Scoopy 110 30400-K2F-ND1"
	};

	private static readonly string[] khEcmId = new string[97]
	{
		"0101F30F11", "0101F30F01", "0102E70F01", "0101700F01", "0102D80F01", "0101340F11", "0101340F01", "01018B0F01", "0100B80F01", "0100B80502",
		"0100B80D01", "0101870F02", "0102E40F01", "0104960F11", "0102360F01", "0102360F11", "01027F0F01", "0102831901", "0104080F01", "0104130F01",
		"0104F10F01", "0104F00F01", "0102040F01", "0101B11100", "0104110F11", "0103250F01", "0101B10F01", "0100FA1001", "0101110F01", "0104530F01",
		"0104290F01", "0803130F01", "0103130F01", "01011B0F02", "0101DA0F01", "0103130F11", "0103100F01", "0104A40F01", "0103110F01", "0104A50F01",
		"0102320F01", "0101CA0F01", "0103920F01", "0104AF0F11", "0104AF0F01", "0100E80F01", "0100E80F02", "0101FA0F01", "0101F80F01", "0102F80F01",
		"0102F70F01", "01002D0F01", "01005F0F01", "0101F30F11", "0104080F01", "0101A10F01", "01011E0F01", "0101C90F01", "0101270D01", "0101C60D01",
		"0104140D01", "0104F30D01", "0104F20D01", "0102810D01", "0102800D01", "01034C0D11", "01034C0D01", "0102B90D01", "0105550D01", "01040F0D01",
		"0101200F01", "0101610D01", "0101880D01", "0100CE0D02", "0104960F01", "01047B0D01", "0102580D01", "01033E0D01", "0101B30D01", "01010E0D01",
		"0102B50D01", "0100DF0D02", "0101010D02", "01047E0D01", "0102130D01", "0101350601", "0101350501", "0101350D01", "0103FA0D01", "0105260D01",
		"01029B1F01", "0102D71F01", "0104421F01", "01041A1F01", "01039A1F01", "0105A70F01", "0105A80F01"
	};

	private static readonly string[] khStartOffset = new string[97]
	{
		"8000", "8000", "8000", "4000", "0", "4000", "4000", "8000", "0", "0",
		"0", "8000", "8000", "8000", "8000", "8000", "0", "0", "0", "0",
		"0", "0", "8000", "8000", "8000", "8000", "8000", "0", "0", "8000",
		"8000", "8000", "8000", "4000", "8000", "8000", "0", "0", "0", "0",
		"8000", "8000", "8000", "8000", "8000", "4000", "4000", "0", "0", "0",
		"0", "0", "0", "8000", "0", "8000", "4000", "8000", "4000", "8000",
		"0", "0", "0", "0", "0", "8000", "8000", "8000", "8000", "8000",
		"4000", "4000", "8000", "4000", "8000", "8000", "8000", "8000", "8000", "4000",
		"8000", "4000", "4000", "8000", "8000", "4000", "4000", "4000", "8000", "8000",
		"8000", "8000", "8000", "8000", "8000", "0", "0"
	};

	private static readonly string[] khCksumOffset = new string[97]
	{
		"0", "0", "0", "7600", "3FFF8", "7600", "7600", "0", "DFEF", "DFEF",
		"DFEF", "0", "0", "0", "0", "0", "3FFF8", "3FFF8", "3FFF8", "3FFF8",
		"3FFF8", "3FFF8", "0", "0", "0", "0", "0", "DFEF", "DFEF", "0",
		"0", "0", "0", "7600", "0", "0", "3FFF8", "3FFF8", "3FFF8", "3FFF8",
		"0", "0", "0", "0", "0", "7600", "7600", "3FFF8", "3FFF8", "3FFF8",
		"3FFF8", "7FEF", "7FEF", "0", "3FFF8", "0", "7600", "0", "7600", "0",
		"3FFF8", "3FFF8", "3FFF8", "3FFF8", "3FFF8", "0", "0", "0", "0", "0",
		"7600", "7600", "0", "7600", "0", "0", "0", "0", "0", "7600",
		"0", "7600", "7600", "0", "0", "7600", "7600", "7600", "0", "0",
		"0", "0", "0", "0", "0", "3FFF8", "3FFF8"
	};

	private static readonly string[] shPartCode = new string[72]
	{
		"BEAT 110-30400-K81A-N02", "BEAT 110-30400-K1AL-NB1", "BEAT 110-30400-K1AL-N91 ISS", "BEAT 110-30400-K1AL-N81-NA1", "ADV150 30400 K0WA-N01 (2018-2021)", "ADV150 30400 K0WA-N11 (2018-2021)", "ADV160 NON ABS K0WL-NA1 (2022-2023)", "ADV160 ABS K0WL-NB1 IN11 (2022-2023)", "ADV160 ABS K0WL-NB1 IN01 (2022-2023)", "BEAT 110 30400 K1AA-N01 (2020-2022)",
		"BEAT 110 30400 K1AA-N02 (2020-2022)", "BEAT 110 30400 K1AA-N03 (2020-2022)", "BEAT 110 30400 K1AA-N11 (2020-2022)", "BEAT 110 30400 K1AA-N12 (2020-2022)", "BEAT 110 30400 K1AA-N13 (2020-2022)", "BEAT 110 30400 K25G-601 (2015-2017)", "BEAT 110 30400 K81A-N02 (2017-2018)", "BEAT 110 30400 K81A-N12 (2018-2019)", "BEAT 110 30400 K81H-N52 (2017-2018)", "BEAT 110 30400 K81H-N62 (2017-2018)",
		"PCX 150 30400 K36J-N11 (2017-2019)", "PCX 150 30400 K97G-N01 (2018-2019)", "PCX 150 30400 K97G-N31 (2019-2020)", "PCX 160 30400 K1ZG-N31 (2020-2021)", "PCX 160 30400 K1ZG-N21 (2020-2021)", "PCX 160 30400 K1ZN-N41 (2023-2023)", "PCX 160 30400 K1ZN-N51 (2023-2023)", "SCOOPY OLD 30400 K25-601 (2016-2018)", "SCOOPY 30400 K25G-611 (2015-2017)", "VARIO 125 NEW K2VG-N42 (2022-2023)",
		"VARIO 125 NEW K2VG-N32 (2022-2023)", "VARIO 125 30400 K60A-B01 (2015-2016)", "VARIO 125 30400 K60K-B11 (2015-2016)", "VARIO 125 30400 K60K-B21 (2016-2018)", "VARIO 125 30400 K60K-B31 (2016-2018)", "VARIO 125 30400 K60R-B61 (2018-2019)", "VARIO 125 30400 K60R-B71 (2018-2019)", "VARIO 150 30400 K59A-A11 (2015-2017)", "VARIO 150 30400 K59F-A01 (2017-2018)", "VARIO 150 30400 K59J-A71 (2018-2021)",
		"VARIO 150 30400 K59J-A711 (2018-2021)", "VARIO 160 30400 K2SA-N02 (2022-2023)", "VARIO 125 OLD KZRA-601 (2011-2012)", "VARIO 125 OLD KZRA-651 (2012-2013)", "VARIO 125 OLD KZRJ-B11 (2012-2013)", "VARIO 125 OLD KZRM-B21 (2012-2013)", "VARIO 125 OLD KZRJ-B31 (2012-2013)", "BEAT 110-30400-K1AL-N81-IN11", "BEAT 110-30400-K1AL-N81-IN01", "ADV 160 THAILAND 30400-K0WM-TA1",
		"ADV 150 THAILAND 30400-K0WF-T01", "PCX 160 THAILAND 30400-K1ZF-T01", "PCX 160 THAILAND 30400-K1ZF-T11", "PCX 160 THAILAND 30400-K1ZP-T51", "PCX 160 THAILAND 30400-K1ZP-T71", "PCX 150 THAILAND 30400-K36F-T01", "PCX 150 THAILAND 30400-K36S-T31", "PCX 150 THAILAND 30400-K97F-T01", "LEAD 125 THAILAND 30400-K2TA-T02", "Zoomer X THAILAND 30400-K20J-T21",
		"Click 160 THAILAND 30400-K2SF-T01", "VARIO 125 THAILAND 30400-K60F-T01", "Click 125 THAILAND 30400-K2VH-T11", "Giorno 125 THAILAND 30400-K3MF-T01", "VARIO 150 THAILAND 30400-K59K-T11", "Click 125i THAILAND 30400-K60S-M61", "Click 125i THAILAND 30400-K60S-T73", "PCX-125 30400-K35A-T01", "PCX-125 30400-K35A-V01", "PCX-150 30400-K97A-U01",
		"BEAT 110 30400 K81A-N11 (2018-2019)", "STYLO K3VA-NO1"
	};

	private static readonly string[] shEcmId = new string[72]
	{
		"0102420F01", "01058E0F01", "01058C0F01", "01058D0F01", "01037B0F01", "01037B0F11", "01048C0F01", "01048D0F11", "01048D0F01", "01037A0F01",
		"01037A0F02", "01037A0F03", "0103790F01", "0103790F02", "0103790F03", "0101D40F01", "0102420F01", "0102410F01", "0102E30F01", "0102E20F01",
		"0101CE0F01", "0102C70F01", "0103470F01", "0104030F01", "0104040F01", "0105030F01", "0105020F01", "0101D30F02", "0101D30F01", "0104D80F01",
		"0104D90F01", "0101E20F01", "0101C50F01", "0102470F01", "0102460F01", "0103080F01", "0103090F01", "0101C40F01", "0102450F01", "0103060F01",
		"0103060F11", "01046B0F01", "0101230F01", "0101730F01", "0101690F01", "0101BE0F01", "0101AE0F01", "01058B0F11", "01058B0F01", "01048E0D01",
		"01037C0D01", "0103D00D01", "0103CF0D01", "0105050D01", "0105040D01", "0101990D01", "0102180D01", "0102990D01", "0104780D01", "0101FF0D01",
		"01046D0D01", "0101D00D01", "0104DC0D01", "01052A0D01", "0103070D01", "01030B0F03", "01030A0D01", "0101980101", "0101981101", "0102970701",
		"0102410F11", "0105770F01"
	};

	private static readonly Dictionary<string, int> ecmIdToComboBoxIndex = new Dictionary<string, int>
	{
		{ "0101D40F01", 0 },
		{ "0102420F01", 1 },
		{ "0102410F11", 2 },
		{ "0102410F01", 3 },
		{ "0102E30F01", 4 },
		{ "0102E20F01", 5 },
		{ "01037A0F01", 6 },
		{ "01037A0F02", 7 },
		{ "01037A0F03", 8 },
		{ "0103790F01", 9 },
		{ "0103790F02", 10 },
		{ "0103790F03", 11 },
		{ "0101E20F01", 12 },
		{ "0101C50F01", 13 },
		{ "0102470F01", 14 },
		{ "0102460F01", 15 },
		{ "0103080F01", 16 },
		{ "0103090F01", 17 },
		{ "0101C40F01", 18 },
		{ "0102450F01", 19 },
		{ "0103060F01", 20 },
		{ "0101230F01", 21 },
		{ "0101730F01", 22 },
		{ "0101690F01", 23 },
		{ "0101BE0F01", 24 },
		{ "0101AE0F01", 25 },
		{ "0104D80F01", 26 },
		{ "0104D90F01", 27 },
		{ "01046B0F01", 28 },
		{ "01037B0F01", 29 },
		{ "01058EOFO1", 30 },
		{ "01058COFO1", 31 },
		{ "0102C70F01", 32 },
		{ "0103470F01", 33 },
		{ "0104040F01", 34 },
		{ "0104030F01", 35 },
		{ "0105030F01", 36 },
		{ "0105020F01", 37 },
		{ "01048C0F01", 38 },
		{ "01048D0F01", 39 },
		{ "01048D0F11", 40 },
		{ "01037B0F11", 41 },
		{ "0101200F01", 42 },
		{ "01011B0F02", 43 },
		{ "0101DA0F01", 44 },
		{ "0103130F01", 45 },
		{ "0101B10F01", 46 },
		{ "0101B11100", 47 },
		{ "0102040F01", 48 },
		{ "0103250F01", 49 },
		{ "0104110F11", 50 },
		{ "0101C90F01", 51 },
		{ "0103920F01", 52 },
		{ "0101CA0F01", 53 },
		{ "0102320F01", 54 },
		{ "0101F30F11", 55 },
		{ "0101F30F01", 56 },
		{ "0101870F02", 57 },
		{ "0101A10F01", 58 },
		{ "0101340F01", 59 },
		{ "01018B0F01", 60 },
		{ "0102E70F01", 61 },
		{ "01027F0F01", 62 },
		{ "0103110F01", 63 },
		{ "0103100F01", 64 },
		{ "0104A40F01", 65 },
		{ "0104A50F01", 66 },
		{ "0104080F01", 67 },
		{ "0104130F01", 68 },
		{ "0104F10F01", 69 },
		{ "0104F00F01", 70 },
		{ "0101FA0F01", 71 },
		{ "0102F80F01", 72 },
		{ "0102F70F01", 73 },
		{ "01011E0F01", 74 },
		{ "0105770F01", 75 },
		{ "0101D30F01", 76 },
		{ "0104530F01", 77 },
		{ "0104AF0F01", 78 },
		{ "0100E80F02", 79 },
		{ "0104290F01", 80 },
		{ "01058B0F11", 81 },
		{ "01058B0F01", 82 },
		{ "0101700F01", 83 },
		{ "01048E0D01", 84 },
		{ "01037C0D01", 85 },
		{ "0103D00D01", 86 },
		{ "0103CF0D01", 87 },
		{ "0105050D01", 88 },
		{ "0105040D01", 89 },
		{ "0101990D01", 90 },
		{ "0102180D01", 91 },
		{ "0102990D01", 92 },
		{ "0104780D01", 93 },
		{ "0101FF0D01", 94 },
		{ "0101270D01", 95 },
		{ "0101C60D01", 96 },
		{ "0104140D01", 97 },
		{ "0104F30D01", 98 },
		{ "0104F20D01", 99 },
		{ "0102810D01", 100 },
		{ "0102800D01", 101 },
		{ "01046D0D01", 102 },
		{ "0101D00D01", 103 },
		{ "0104DC0D01", 104 },
		{ "01052A0D01", 105 },
		{ "0103070D01", 106 },
		{ "01030B0F03", 107 },
		{ "01030A0D01", 108 },
		{ "01034C0D11", 109 },
		{ "01034C0D01", 110 },
		{ "0102E40F01", 111 },
		{ "0102B90D01", 112 },
		{ "0105550D01", 113 },
		{ "01040F0D01", 114 },
		{ "0101610D01", 115 },
		{ "0101880D01", 116 },
		{ "0100CE0D02", 117 },
		{ "0104960F01", 118 },
		{ "0104960F11", 119 },
		{ "0100E80F01", 120 },
		{ "0104AF0F11", 121 },
		{ "01047B0D01", 122 },
		{ "0102580D01", 123 },
		{ "01033E0D01", 124 },
		{ "0101B30D01", 125 },
		{ "01010E0D01", 126 },
		{ "0102B50D01", 127 },
		{ "0100DF0D02", 128 },
		{ "0101010D02", 129 },
		{ "01047E0D01", 130 },
		{ "0102130D01", 131 },
		{ "0101350601", 132 },
		{ "0101350501", 133 },
		{ "0101350D01", 134 },
		{ "0103FA0D01", 135 },
		{ "0105260D01", 136 },
		{ "01058D0F01", 137 },
		{ "0101980101", 138 },
		{ "0101981101", 139 },
		{ "0102970701", 140 },
		{ "01029B1F01", 141 },
		{ "0102D71F01", 142 },
		{ "0104421F01", 143 },
		{ "01041A1F01", 144 },
		{ "01039A1F01", 145 },
		{ "0101F80F01", 146 },
		{ "0105B80F01", 147 },
		{ "0105B70F01", 148 },
		{ "0105B60F01", 149 },
		{ "0105A70F01", 150 },
		{ "0105A80F01", 151 }
	};

	private bool showPointer = false;

	private List<double> mapValues = new List<double>();

	private List<double> tpsValues = new List<double>();

	private List<double> rpmValues = new List<double>();

	private int RPMD;

	private float TPSD;

	private float MAP;

	private float CL;

	private float AD;

	private float inj;

	private float IDC;

	private int activeAddress = -1;

	private byte activeMask = 0;

	private string currentMenu;

	private const string KeyFilePath = "license.lic";

	private readonly byte[] Key = Encoding.UTF8.GetBytes("ABCDEF0123456789");

	private readonly byte[] IV = Encoding.UTF8.GetBytes("0123456789ABCDEF");

	private bool isCompared = false;

	private System.Windows.Forms.Timer labelMovementTimer;

	private const int LabelSpeed = 5;

	private int labelOffset = 0;

	private bool isTaskRunning = false;

	private System.Windows.Forms.Timer updateTimer;

	private Chart chartForm;

	private CancellationTokenSource _cancellationTokenSource;

	private bool tek10 = true;

	private bool tek11 = true;

	private bool tek13 = true;

	private bool tek16 = true;

	private bool tek17 = true;

	private int pointerRow = 1;

	private int pointerColumn = 1;

	private static int ukuranpointer;

	private System.Windows.Forms.Timer moveTimer;

	private int oldPointerRow = -1;

	private int oldPointerColumn = -1;

	private float nilaiPersen;

	private float NILAI;

	private float O2V2;

	private float O2V;

	private float STF;

	private float AFR;

	private float O2V_mapped_0_1;

	private float O2V_mapped_9_5_20;

	private Dictionary<(int, int), double> initialCellValues = new Dictionary<(int, int), double>();

	private Thread workerThread;

	private int selectedRowIndex;

	private _2d form2d;

	private float cc;

	private float HP;

	private float TQ;

	private bool liv = false;

	public DYNO dynoForm;

	private IContainer components = null;

	private Button show_all;

	private DataGridView dataGridView1;

	private ComboBox comboBox1;

	private Button button1;

	private ProgressBar PbProgress;

	private Button BtnWrite;

	private Label TxtConnStat;

	private SaveFileDialog saveFileDialog1;

	private Label label5;

	private Label label4;

	private Button button3;

	private Label label7;

	private Label label6;

	private Label TxtPartCode;

	private TextBox TbStartOffset;

	private TextBox TbChecksumOffset;

	private TextBox TbFileName;

	private Label TxtFlashCount;

	private CheckBox CbFastWrite;

	private Panel panel4;

	private Label label8;

	private Button button8;

	private Label label11;

	private Button button12;

	private AGauge aGauge4;

	private AGauge aGauge3;

	private MenuStrip menuStrip7;

	private ToolStripMenuItem fileToolStripMenuItem;

	private ToolStripMenuItem diagnosticToolStripMenuItem;

	private ToolStripMenuItem resetCountToolStripMenuItem;

	private ToolStripMenuItem breakpointToolStripMenuItem1;

	private ToolStripMenuItem toolStripMenuItem23;

	private ToolStripMenuItem breakpointToolStripMenuItem2;

	private ToolStripMenuItem fueToolStripMenuItem;

	private ToolStripMenuItem ignitionToolStripMenuItem;

	private ToolStripMenuItem limiterToolStripMenuItem;

	private ToolStripMenuItem settinganLaiinToolStripMenuItem;

	private ToolStripMenuItem breakpoinRPMToolStripMenuItem;

	private ToolStripMenuItem breakpointTPSToolStripMenuItem;

	private ToolStripMenuItem breakpointIAPToolStripMenuItem;

	private ToolStripMenuItem rpmVsTps1ToolStripMenuItem;

	private ToolStripMenuItem rpmVsTps2ToolStripMenuItem;

	private ToolStripMenuItem rpmVsToolStripMenuItem;

	private ToolStripMenuItem rpmVsIAPToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem6;

	private ToolStripMenuItem toolStripMenuItem7;

	private ToolStripMenuItem toolStripMenuItem8;

	private ToolStripMenuItem ignition4ToolStripMenuItem;

	private ToolStripMenuItem ignition5ToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem10;

	private ToolStripMenuItem toolStripMenuItem11;

	private ToolStripMenuItem toolStripMenuItem12;

	private ToolStripMenuItem toolStripMenuItem13;

	private ToolStripMenuItem toolStripMenuItem14;

	private ToolStripMenuItem limiter6ToolStripMenuItem;

	private ToolStripMenuItem limiter7ToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem16;

	private ToolStripMenuItem toolStripMenuItem17;

	private ToolStripMenuItem toolStripMenuItem18;

	private ToolStripMenuItem toolStripMenuItem19;

	private ToolStripMenuItem toolStripMenuItem20;

	private ToolStripMenuItem toolStripMenuItem21;

	private ToolStripMenuItem toolStripMenuItem22;

	private ToolStripMenuItem offMode4ToolStripMenuItem;

	private ToolStripMenuItem offBASSensorToolStripMenuItem;

	private ToolStripMenuItem offMAPSensorToolStripMenuItem;

	private ToolStripMenuItem offECTSensorToolStripMenuItem;

	private ToolStripMenuItem offIATSensorToolStripMenuItem;

	private ToolStripMenuItem offVSSSensorToolStripMenuItem;

	private ToolStripMenuItem offMAPSensorToolStripMenuItem1;

	private ComboBox comboBox2;

	private GroupBox groupBox1;

	private GroupBox groupBox2;

	private GroupBox groupBox3;

	private GroupBox groupBox4;

	private GroupBox groupBox5;

	private GroupBox groupBox6;

	private ToolStripMenuItem openFileToolStripMenuItem;

	private ToolStripMenuItem button9;

	private ToolStripMenuItem saveAsToolStripMenuItem;

	private Label TxtEcmId;

	private ToolStripMenuItem keihinToolStripMenuItem;

	private ToolStripMenuItem shindengenToolStripMenuItem;

	private Label label15;

	private Label label16;

	private Label label17;

	private Label label18;

	private PictureBox pictureBox1;

	private Label label1;

	private Label TxtPb;

	private Label label3;

	private Label label14;

	private ToolStripMenuItem readIDBarcodeToolStripMenuItem;

	private System.Windows.Forms.Timer timer1;

	private ToolStripMenuItem convertHexToIDKeyYamahaToolStripMenuItem;

	private ToolStripMenuItem unlockPasswordXdfToolStripMenuItem;

	private Button button2;

	private TextBox txtInput;

	private Label label19;

	private Label label20;

	private GroupBox groupBox7;

	private System.Windows.Forms.Timer timer2;

	private Label label2;

	private Label label9;

	private Label label10;

	private Button button4;

	private Button button6;

	private TextBox textBox1;

	private TextBox textBox2;

	private Label label12;

	private Label label13;

	private Button button10;

	private Panel panelDATA;

	private static int PointerSize
	{
		get
		{
			return ukuranpointer;
		}
		set
		{
			ukuranpointer = value;
		}
	}

	public Form1()
	{
		InitializeComponent();
		DoubleBuffered = true;
		Opacity = 0.0;
		try
		{
			Image startupBackground = LoadEmbeddedImage("EcuTuneX.bg_h2m.jpg");
			if (startupBackground != null)
			{
				panelDATA.BackgroundImage = startupBackground;
				panelDATA.BackgroundImageLayout = ImageLayout.Stretch;
			}
		}
		catch (Exception)
		{
		}
		Shown += delegate
		{
			System.Windows.Forms.Timer revealTimer = new System.Windows.Forms.Timer();
			revealTimer.Interval = 350;
			revealTimer.Tick += delegate
			{
				revealTimer.Stop();
				revealTimer.Dispose();
				Opacity = 1.0;
			};
			revealTimer.Start();
		};
		Load += delegate
		{
			ExtraStartup();
		};
		txtInput.Text = initialValue;
		IsConfirmed = false;
		timer1.Interval = 500;
		timer1.Enabled = true;
		isFastWrite = true;
		FormClosing += MainForm_Closing;
		Load += MainForm_Load;
		TbChecksumOffset.TextChanged += ChecksumOffset_Changed;
		TbStartOffset.TextChanged += StartOffset_Changed;
		CbFastWrite_CheckedChanged(CbFastWrite, EventArgs.Empty);
		Visible = false;
		CreateTable();
		PrintResourceNames();
		dataGridView1.CellPainting += DataGridView1_CellPainting;
		isivom();
		isieditvalue();
		comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
		dataGridView1.AllowUserToAddRows = false;
		dataGridView1.Visible = false;
		numberOfRows = columnCount;
		numberOfColumnss = rowCount;
		AddClickHandlers(menuStrip7.Items);
		dataGridView1.CellClick += DataGridView1_CellClick;
		dataGridView1.SelectionChanged += DataGridView1_SelectionChangedSync;
		EnableDoubleBuffering(dataGridView1);
		Resize += Form1_Resize;
		dataGridView1.KeyDown += dataGridView1_KeyDown;
		form2d = new _2d();
		form2d.DataPointValueChanged += Form2d_DataPointValueChanged;
		KeyDown += Form1_KeyDown;
		InitializeContextMenu();
		InitializeLabelMovement();
	}

	private void btnOK_Click_1(object sender, EventArgs e)
	{
		if (comboBox2.SelectedItem == null)
		{
			return;
		}
		if (comboBox2.SelectedItem.ToString() == "Copy from Compare")
		{
			CopyFromCompare();
			return;
		}
		RunWithRealValues(() => ExecuteFunctionCore(sender, e));
	}

	private void ExecuteFunctionCore(object sender, EventArgs e)
	{
		switch (comboBox2.SelectedItem.ToString())
		{
		case "Offset":
			if (binBytes != null)
			{
				button9.Enabled = true;
				MasukkanNilaiBaru(sender, e);
				SaveEditedValues2();
				SaveState();
				txtInput.Text = "";
			}
			break;
		case "Offset (+/-)":
			if (binBytes != null)
			{
				TambahNilai(sender, e);
				SaveEditedValues2();
				SaveState();
				txtInput.Text = "";
			}
			break;
		case "Offset (-)":
			if (binBytes != null)
			{
				button9.Enabled = true;
				KurangiNilai(sender, e);
				SaveEditedValues2();
				SaveState();
				txtInput.Text = "";
			}
			break;
		case "Scala by %":
			if (binBytes != null)
			{
				KaliNilai(sender, e);
				SaveEditedValues2();
				SaveState();
				txtInput.Text = "";
			}
			break;
		case "(-)Scala by %":
			if (binBytes != null)
			{
				BagiNilai(sender, e);
				SaveEditedValues2();
				SaveState();
				txtInput.Text = "";
			}
			break;
		case "Smooth":
			if (binBytes != null)
			{
				TambahNilaiInterpolasi(sender, e);
				SaveEditedValues2();
				SaveState();
				txtInput.Text = "";
			}
			break;
		}
	}

	private void Form1_Resize(object sender, EventArgs e)
	{
		if (ukur)
		{
			UpdateDataGridViewSize();
		}
	}

	private void UpdateDataGridViewSize()
	{
		if (dataGridView1.Rows.Count <= 0)
		{
			return;
		}
		dataGridView1.Dock = DockStyle.None;
		dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		int num = 60;
		dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
		foreach (DataGridViewColumn column in dataGridView1.Columns)
		{
			column.Width = num;
			column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
		}
		int num2 = 35;
		foreach (DataGridViewRow item in (IEnumerable)dataGridView1.Rows)
		{
			item.Height = num2;
		}
		dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
		dataGridView1.ColumnHeadersHeight = 30;
		float emSize = 10f;
		Font font = new Font("Arial", emSize, FontStyle.Regular);
		dataGridView1.DefaultCellStyle.Font = font;
		dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
		dataGridView1.ColumnHeadersDefaultCellStyle.Font = font;
		dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
		dataGridView1.Width = panelDATA.ClientSize.Width;
		dataGridView1.Height = panelDATA.ClientSize.Height;
		dataGridView1.Refresh();
	}

	private void EnableDoubleBuffering(Control control)
	{
		typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(control, true, null);
	}

	private void MainForm_Load(object sender, EventArgs e)
	{
		isConnected = true;
		isNoInterface = false;
		CbFastWrite.Checked = isFastWrite;
		CbFastWrite.Checked = true;
	}

	private byte[] DecryptBytes2(byte[] encryptedBytes)
	{
		using Aes aes = Aes.Create();
		aes.Key = Key;
		aes.IV = IV;
		ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
		using MemoryStream stream = new MemoryStream(encryptedBytes);
		using CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read);
		using MemoryStream memoryStream = new MemoryStream();
		cryptoStream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	private void button2_Click(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Title = "Open File",
			Filter = "Files (*.bin)|*.bin|Binary Files (*.acg)|*.acg"
		};
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			try
			{
				byte[] previousBytes = binBytes;
				string text = Path.GetExtension(openFileDialog.FileName).ToLower();
				if (text == ".acg")
				{
					byte[] encryptedBytes = File.ReadAllBytes(openFileDialog.FileName);
					byte[] array = DecryptBytes2(encryptedBytes);
					binBytes = array;
				}
				else
				{
					if (!(text == ".bin"))
					{
						MessageBox.Show("Invalid file type selected. Please choose a .bin or .binx file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
						return;
					}
					binBytes = File.ReadAllBytes(openFileDialog.FileName);
				}
				if (!ConfirmBinMatchesSelection(binBytes, "membuka file ini"))
				{
					binBytes = previousBytes;
					return;
				}
				binSize = binBytes.Length;
				TbFileName.Text = openFileDialog.FileName;
				if (baseTitle.Length > 0)
				{
					Text = baseTitle + "  |  " + Path.GetFileName(openFileDialog.FileName);
				}
				if (isUnknown)
				{
					GetOffsetsFromBinType();
				}
			}
			catch (Exception)
			{
			}
		}
		else
		{
			TbFileName.Text = "";
			binBytes = null;
			binSize = 0;
		}
		undoStack.Clear();
		redoStack.Clear();
		SaveState();
		UpdateBinIdentity();
	}

	private void GetOffsetsFromBinType()
	{
		if (binSize == 49152)
		{
			TbStartOffset.Text = "4000";
			TbChecksumOffset.Text = "7600";
		}
		else if (binSize == 57344)
		{
			TbStartOffset.Text = "0";
			TbChecksumOffset.Text = "DFEF";
		}
		else if (binSize == 65536 || binSize == 98304)
		{
			TbStartOffset.Text = "8000";
			TbChecksumOffset.Text = "0";
		}
		else if (binSize == 131072)
		{
			TbStartOffset.Text = "E0000";
			TbChecksumOffset.Text = "B000";
		}
		else
		{
			if (binSize < 262144)
			{
				return;
			}
			if (CompareMemory(binBytes, binSize - shBinSig.Length, shBinSig, shBinSig.Length))
			{
				for (int i = 0; i < binSize - 5; i++)
				{
					if (CompareMemory(binBytes, i, shV850Sig, shV850Sig.Length) || CompareMemory(binBytes, i, shRh850Sig, shRh850Sig.Length))
					{
						TbStartOffset.Text = "auto";
						TbChecksumOffset.Text = "auto";
						break;
					}
				}
			}
			else if (binSize == 262144 && (CompareMemory(binBytes, binSize - khRh850Sig.Length, khRh850Sig, khRh850Sig.Length) || CompareMemory(binBytes, binSize - khM32rSig.Length, khM32rSig, khM32rSig.Length) || CompareMemory(binBytes, binSize - khMpc560Sig.Length, khMpc560Sig, khMpc560Sig.Length)))
			{
				TbStartOffset.Text = "0";
				TbChecksumOffset.Text = "3FFF8";
			}
			else if (binSize == 524288)
			{
				if (CompareMemory(binBytes, binSize - khMpc560Sig.Length, khMpc560Sig, khMpc560Sig.Length))
				{
					TbStartOffset.Text = "0";
					TbChecksumOffset.Text = "7FFF8";
				}
			}
			else if (binSize == 1048576)
			{
				if (CompareMemory(binBytes, binSize - khSH705XSig.Length, khSH705XSig, khSH705XSig.Length))
				{
					TbStartOffset.Text = "0";
					TbChecksumOffset.Text = "FFFF8";
				}
				else
				{
					TbStartOffset.Text = "0";
					TbChecksumOffset.Text = "7FFF8";
				}
			}
		}
	}

	private bool InitFtdiIso()
	{
		byte[] array = new byte[1];
		byte[] array2 = new byte[1] { 1 };
		uint uint_ = 0u;
		if (FTDI.FT_Open(0u, ref ftHandle) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_Purge(ftHandle, 3u) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_SetBitMode(ftHandle, 0, 0) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_SetDataCharacteristics(ftHandle, 8, 0, 0) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_SetBaudRate(ftHandle, 10400u) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_SetTimeouts(ftHandle, 50u, 0u) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_SetLatencyTimer(ftHandle, 8) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_SetBitMode(ftHandle, 1, 1) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_Write(ftHandle, array, (uint)array.Length, ref uint_) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		Thread.Sleep(70);
		if (FTDI.FT_Write(ftHandle, array2, (uint)array2.Length, ref uint_) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_SetBitMode(ftHandle, 0, 0) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_SetBaudRate(ftHandle, 10400u) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		if (FTDI.FT_Purge(ftHandle, 3u) > FTDI.FT_STATUS.FT_OK)
		{
			FTDI.FT_Close(ftHandle);
			return false;
		}
		Thread.Sleep(130);
		return true;
	}

	private void WorkerThread()
	{
		byte[] array = new byte[4] { 254, 4, 114, 140 };
		byte[] array2 = new byte[5] { 114, 5, 0, 240, 153 };
		byte[] array3 = new byte[7] { 114, 7, 114, 0, 0, 5, 16 };
		byte[] array4 = new byte[6] { 125, 6, 1, 1, 3, 120 };
		byte[] array5 = new byte[6] { 123, 6, 2, 1, 3, 121 };
		byte[] array6 = new byte[6] { 126, 6, 1, 1, 0, 122 };
		byte[] array7 = new byte[6] { 124, 6, 2, 1, 0, 123 };
		byte[] array8 = new byte[6] { 114, 5, 0, 240, 153, 0 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		if (!con)
		{
			return;
		}
		if (keihinResetRunning)
		{
			return;
		}
		if (InitFtdiIso())
		{
			if (isNoInterface)
			{
				isConnected = true;
				isNoInterface = false;
			}
			SendCommand(array, array.Length, ref reply, ref replySize);
			if (SendCommand(array2, array2.Length, ref reply, ref replySize))
			{
				array8 = new byte[5] { 114, 5, 0, 240, 153 };
			}
			else if (SendCommand(array6, array6.Length, ref reply, ref replySize) && reply[3] != 15)
			{
				array8 = new byte[6] { 126, 6, 1, 1, 0, 122 };
			}
			if (SendCommand(array8, array8[1], ref reply, ref replySize))
			{
				string newText = "-";
				isConnected = true;
				if (SendCommand(array3, array3.Length, ref reply, ref replySize))
				{
					text = BitConverter.ToString(reply, 5, 5).Replace("-", "");
				}
				if (SendCommand(array4, array4.Length, ref reply, ref replySize))
				{
					newText = reply[6].ToString("D");
					Debug.WriteLine("Response from array4: " + BitConverter.ToString(reply, 0, (int)replySize));
				}
				if (SendCommand(array5, array5.Length, ref reply, ref replySize))
				{
					newText = reply[6].ToString("D");
					Debug.WriteLine("Response from array5: " + BitConverter.ToString(reply, 0, (int)replySize));
				}
				SetControlText(TxtConnStat, "Connected to ECM");
				SetControlText(TxtEcmId, text);
				SetControlText(TxtFlashCount, newText);
				if (SetEcmParamsFromDb(text) == 0)
				{
					isRecovery = true;
				}
				con = false;
			}
			else if (isConnected)
			{
				isConnected = false;
				SetControlText(TxtConnStat, "Disconnected to ECM");
				SetControlText(TxtEcmId, "-");
				SetControlText(TxtPartCode, "Please Kontak ON\r\nECM NOT CONNECT");
				SetControlText(TxtFlashCount, "-");
				SetControlText(TbStartOffset, "Auto");
				SetControlText(TbChecksumOffset, "Auto");
				SetControlText(TbFileName, "");
			}
			FTDI.FT_Close(ftHandle);
		}
		else if (!isNoInterface)
		{
			isNoInterface = true;
			SetControlText(TxtConnStat, "No Interface");
			SetControlText(TxtEcmId, "-");
			SetControlText(TxtPartCode, "Ecu Tunex\r\nNo Interface");
			SetControlText(TxtFlashCount, "-");
			SetControlText(TbStartOffset, "Auto");
			SetControlText(TbChecksumOffset, "Auto");
			SetControlText(TbFileName, "");
		}
	}

	private void flas()
	{
		string serialNumber = GetProcessorId().Trim();
		string text = ProcessDiskSerial(serialNumber);
		string[] array = DecryptKeyFromFile();
		string text2 = array[0];
		string text3 = array[1];
		string text4 = array[2];
		if (text != text2 && false)
		{
			return;
		}
		byte[] array2 = new byte[4] { 254, 4, 114, 140 };
		byte[] array3 = new byte[5] { 114, 5, 0, 240, 153 };
		byte[] array4 = new byte[6] { 126, 6, 1, 1, 0, 122 };
		byte[] array5 = new byte[6] { 124, 6, 2, 1, 0, 123 };
		byte[] array6 = new byte[6] { 114, 5, 0, 240, 153, 0 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		if (!InitFtdiIso())
		{
			return;
		}
		SendCommand(array2, array2.Length, ref reply, ref replySize);
		SendCommand(array3, array3.Length, ref reply, ref replySize);
		ECUTYPE ecuType = GetEcuType();
		if (ecuType == ECUTYPE.SHINDENGENV850 || ecuType == ECUTYPE.SHINDENGENRH850 || ecuType == ECUTYPE.SHINDENGENRH850B)
		{
			ShindengenAutoChecksum(ecuType);
			if (ShindengenPreWriteFlash(isFastWrite, isRecovery))
			{
				WriteFlash(startOffset, ecuType, isFastWrite);
			}
			while (SendCommand(array5, array5.Length, ref reply, ref replySize))
			{
				if (endThread)
				{
					return;
				}
				Thread.Sleep(1000);
			}
		}
		else
		{
			KeihinAutoChecksum(checksumOffset);
			if (KeihinPreWriteFlash())
			{
				WriteFlash(startOffset, ecuType, isFastWrite);
			}
			while (SendCommand(array4, array4.Length, ref reply, ref replySize))
			{
				if (endThread)
				{
					return;
				}
				Thread.Sleep(1000);
			}
		}
		FTDI.FT_Close(ftHandle);
	}

	private bool SendCommand(byte[] command, int commandSize, ref byte[] reply, ref uint replySize, int sleepMs = 0)
	{
		byte[] array = new byte[256];
		byte[] array2 = new byte[256];
		uint uint_ = 0u;
		uint uint_2 = 0u;
		uint uint_3 = 0u;
		uint num = 0u;
		uint num2 = 0u;
		long num3 = 50 + 2 * commandSize;
		bool result;
		if (FTDI.FT_Write(ftHandle, command, (uint)commandSize, ref uint_) > FTDI.FT_STATUS.FT_OK)
		{
			result = false;
		}
		else if (FTDI.FT_SetLatencyTimer(ftHandle, 8) <= FTDI.FT_STATUS.FT_OK)
		{
			if (sleepMs > 0)
			{
				Thread.Sleep(sleepMs);
			}
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while (true)
			{
				if (FTDI.FT_GetQueueStatus(ftHandle, ref uint_2) == FTDI.FT_STATUS.FT_OK && uint_2 != 0 && (ulong)uint_2 < (ulong)array.Length && FTDI.FT_Read(ftHandle, array, uint_2, ref uint_3) == FTDI.FT_STATUS.FT_OK && uint_3 != 0)
				{
					Array.Copy(array, 0L, array2, num, uint_3);
					num += uint_3;
					if ((ulong)num >= (ulong)(commandSize + 2))
					{
						if (num2 == 0)
						{
							num2 = array2[commandSize + 1];
						}
						if (num - commandSize == num2)
						{
							replySize = num2;
							Array.Copy(array2, num - num2, reply, 0L, num2);
							if (ValidateChecksum(reply, (int)num2))
							{
								break;
							}
						}
					}
				}
				if (stopwatch.ElapsedMilliseconds < num3)
				{
					continue;
				}
				stopwatch.Stop();
				FTDI.FT_Purge(ftHandle, 3u);
				return false;
			}
			stopwatch.Stop();
			FTDI.FT_Purge(ftHandle, 3u);
			result = true;
		}
		else
		{
			result = false;
		}
		return result;
	}

	private void SetControlText(Control control, string newText)
	{
		if (control.InvokeRequired)
		{
			SetControlTextCallBack method = SetControlText;
			control.Invoke(method, control, newText);
		}
		else
		{
			control.Text = newText;
		}
	}

	private int SetEcmParamsFromDb(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			MessageBox.Show("ID tidak valid", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return -1;
		}
		int result = -1;
		if (id.EndsWith("0000"))
		{
			SetControlText(TxtPartCode, "Recovery mode");
			SetControlText(TbStartOffset, "Auto");
			SetControlText(TbChecksumOffset, "Auto");
			isUnknown = true;
			isRecovery = true;
			result = 0;
		}
		else
		{
			bool flag = false;
			for (int i = 0; i < khEcmId.Length; i++)
			{
				if (khEcmId[i].Equals(id))
				{
					SetControlText(TxtPartCode, khPartCode[i]);
					SetControlText(TbStartOffset, khStartOffset[i]);
					SetControlText(TbChecksumOffset, khCksumOffset[i]);
					isUnknown = false;
					result = 1;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				for (int j = 0; j < shEcmId.Length; j++)
				{
					if (shEcmId[j].Equals(id))
					{
						SetControlText(TxtPartCode, shPartCode[j]);
						SetControlText(TbStartOffset, "Auto");
						SetControlText(TbChecksumOffset, "Auto");
						isUnknown = false;
						result = 1;
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				SetControlText(TxtPartCode, "Unknown Part Code");
				SetControlText(TbStartOffset, "Auto");
				SetControlText(TbChecksumOffset, "Auto");
				isUnknown = true;
				result = 1;
				MessageBox.Show("Database Tidak Ditemukan ", "Tidak tersedia", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
			if (ecmIdToComboBoxIndex.ContainsKey(id) && id2 != this.text)
			{
				int num = ecmIdToComboBoxIndex[id];
				if (comboBox1.Items.Count > num)
				{
					string text = comboBox1.Items[num].ToString();
					DialogResult dialogResult = MessageBox.Show("Base Map Tersedia\nBase Map: " + text, "Base Map", MessageBoxButtons.OK, MessageBoxIcon.Question);
					if (dialogResult == DialogResult.OK)
					{
						SetComboBoxSelectedIndex(comboBox1, num);
						MAAP = true;
						id2 = this.text;
					}
				}
				else
				{
					SetComboBoxSelectedIndex(comboBox1, -1);
				}
			}
		}
		return result;
	}

	private void SetComboBoxSelectedIndex(ComboBox comboBox, int index)
	{
		if (comboBox.InvokeRequired)
		{
			comboBox.Invoke((Action)(() =>
			{
				comboBox.SelectedIndex = index;
			}));
		}
		else
		{
			comboBox.SelectedIndex = index;
		}
	}

	private ECUTYPE GetEcuType()
	{
		if (CompareMemory(binBytes, binSize - shBinSig.Length, shBinSig, shBinSig.Length))
		{
			bool flag = false;
			for (int i = 0; i < binSize - shWKeysig.Length; i++)
			{
				if (CompareMemory(binBytes, i, shWKeysig, shWKeysig.Length))
				{
					flag = true;
				}
				if (CompareMemory(binBytes, i, shV850Sig, shV850Sig.Length))
				{
					SetControlText(TbStartOffset, "10000");
					return ECUTYPE.SHINDENGENV850;
				}
				if (CompareMemory(binBytes, i, shRh850Sig, shRh850Sig.Length))
				{
					if (CompareMemory(binBytes, 40960, shStart0, shStart0.Length))
					{
						SetControlText(TbStartOffset, "A000");
						return ECUTYPE.SHINDENGENRH850B;
					}
					SetControlText(TbStartOffset, "10000");
					return ECUTYPE.SHINDENGENRH850;
				}
			}
			if (flag)
			{
				SetControlText(TbStartOffset, "10000");
				return ECUTYPE.SHINDENGENV850;
			}
		}
		bool flag2 = (binSize == 57344 || binSize == 32768) && CompareMemory(binBytes, binSize - khH8cSig.Length, khH8cSig, khH8cSig.Length);
		return flag2 ? ECUTYPE.KEIHINH8C : ((binSize == 262144 && CompareMemory(binBytes, binSize - khM32rSig.Length, khM32rSig, khM32rSig.Length)) ? ECUTYPE.KEIHINM32R : ECUTYPE.KEIHINOTHER);
	}

	private void ShindengenAutoChecksum(ECUTYPE ecuTYPE)
	{
		for (int i = 0; i < binSize - shWKeysig.Length; i++)
		{
			if (!CompareMemory(binBytes, i, shWKeysig, shWKeysig.Length))
			{
				continue;
			}
			for (int j = i + shWKeysig.Length; j < binSize - 6; j++)
			{
				if (binBytes[j] == 208 && binBytes[j + 1] == 7)
				{
					int num = j;
					if (ecuTYPE == ECUTYPE.SHINDENGENRH850B)
					{
						num = j - 6;
					}
					SetControlText(TbChecksumOffset, (num - 4).ToString("X"));
					uint num2 = Sum32Bit(binBytes, num - 4) + Sum32Bit(binBytes, binSize - num, num);
					byte[] bytes = BitConverter.GetBytes(0 - num2);
					binBytes[num - 4] = bytes[0];
					binBytes[num - 3] = bytes[1];
					binBytes[num - 2] = bytes[2];
					binBytes[num - 1] = bytes[3];
					Debug.WriteLine("ECU Type: " + ecuTYPE.ToString() + " | " + "Address " + (num - 4).ToString("X") + " bytes " + bytes[0].ToString("X2") + " " + bytes[1].ToString("X2") + " " + bytes[2].ToString("X2") + " " + bytes[3].ToString("X2"));
					return;
				}
			}
		}
	}

	private bool ShindengenPreWriteFlash(bool fastWrite = false, bool recoveryMode = false)
	{
		byte[] array = new byte[8] { 123, 8, 2, 2, 80, 25, 118, 154 };
		byte[] array2 = new byte[8] { 123, 8, 2, 3, 37, 55, 134, 150 };
		byte[] array3 = new byte[5] { 123, 5, 2, 4, 122 };
		byte[] array4 = new byte[7] { 123, 7, 2, 5, 0, 0, 0 };
		byte[] array5 = new byte[6] { 124, 6, 2, 1, 0, 123 };
		byte[] array6 = new byte[11]
		{
			124, 11, 2, 11, 0, 0, 0, 255, 255, 255,
			111
		};
		byte[] array7 = new byte[7] { 124, 7, 2, 14, 1, 0, 108 };
		byte[] array8 = new byte[6] { 124, 6, 2, 4, 255, 121 };
		byte[] array9 = new byte[5] { 124, 5, 2, 5, 120 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		int sleepMs = 0;
		if (!fastWrite)
		{
			sleepMs = 150;
		}
		SendCommand(array, array.Length, ref reply, ref replySize, sleepMs);
		SendCommand(array2, array2.Length, ref reply, ref replySize, sleepMs);
		for (int i = 0; i < 10; i++)
		{
			SetControlText(TxtPb, "⏳ " + (10 - i).ToString("D"));
			SetProgressBarPosition(PbProgress, 10000 - 1000 * i);
			Thread.Sleep(1500);
			if (i == 9)
			{
				SetProgressBarPosition(PbProgress, 0);
				Thread.Sleep(500);
			}
		}
		SendCommand(array3, array3.Length, ref reply, ref replySize, sleepMs);
		bool result;
		if (reply[1] == 7 && reply[3] == 0)
		{
			byte[] array10 = new byte[2];
			byte[] key = new byte[2];
			array10[0] = reply[4];
			array10[1] = reply[5];
			if ((!recoveryMode) ? GetShindengenWriteKey(array10, ref key) : GetShindengenRecoveryKey(array10, ref key))
			{
				array4[4] = key[0];
				array4[5] = key[1];
				array4[6] = Checksum8BitHonda(array4, reply[1] - 1);
			}
			SendCommand(array4, array4.Length, ref reply, ref replySize, sleepMs);
			if (reply[1] == 5 && reply[3] == 0)
			{
				SendCommand(array5, array5.Length, ref reply, ref replySize, sleepMs);
				SendCommand(array6, array6.Length, ref reply, ref replySize, sleepMs);
				SendCommand(array7, array7.Length, ref reply, ref replySize, sleepMs);
				if (SendCommand(array8, array8.Length, ref reply, ref replySize, sleepMs) && reply[3] == 0)
				{
					Thread.Sleep(2000);
					if (SendCommand(array9, array9.Length, ref reply, ref replySize, sleepMs) && reply[3] == 0)
					{
						return true;
					}
					if (reply[3] == 250)
					{
						MessageBox.Show("Erasing flash failed, block error", "Write Flash", MessageBoxButtons.OK, MessageBoxIcon.Hand);
						return false;
					}
				}
				MessageBox.Show("Erasing flash failed", "Write Flash", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				result = false;
			}
			else
			{
				MessageBox.Show("Failed challenging Seed/Key", "Write Flash", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				result = false;
			}
		}
		else
		{
			MessageBox.Show("Failed generating Seed/Key", "Write Flash", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			result = false;
		}
		return result;
	}

	private void WriteFlash(int startOffset, ECUTYPE ecuType, bool isFastWrite)
	{
		byte[] array = new byte[6] { 255, 37, 0, 32, 124, 0 };
		byte[] array2 = new byte[6] { 126, 6, 1, 1, 0, 122 };
		byte[] array3 = new byte[5] { 126, 5, 1, 7, 117 };
		byte[] array4 = new byte[5] { 126, 5, 1, 8, 116 };
		byte[] array5 = new byte[5] { 126, 5, 1, 9, 115 };
		byte[] array6 = new byte[5] { 126, 5, 1, 10, 114 };
		byte[] array7 = new byte[5] { 126, 5, 1, 12, 112 };
		byte[] array8 = new byte[5] { 126, 5, 1, 13, 111 };
		byte[] array9 = new byte[5] { 124, 5, 2, 8, 117 };
		byte[] array10 = new byte[5] { 124, 5, 2, 9, 116 };
		byte[] array11 = new byte[5] { 124, 5, 2, 10, 115 };
		byte[] array12 = new byte[5] { 124, 5, 2, 12, 113 };
		byte[] array13 = new byte[5] { 124, 5, 2, 13, 112 };
		byte[] reply = new byte[256];
		byte[] array14 = new byte[256];
		byte[] array15 = new byte[128];
		uint replySize = 0u;
		int sleepMs = 0;
		int num = 0;
		int num2 = 128;
		int num3 = 8;
		int num4 = startOffset / 16;
		int num5 = 0;
		int num6 = 0;
		int num7 = binSize / 128;
		Stopwatch stopwatch = new Stopwatch();
		TimeSpan timeSpan = default;
		bool flag = true;
		array14[0] = 126;
		array14[1] = 139;
		array14[2] = 1;
		array14[3] = 6;
		for (int i = 0; i < array15.Length; i++)
		{
			array15[i] = byte.MaxValue;
		}
		Console.WriteLine($"ecuType: {ecuType}");
		Console.WriteLine($"fastWrite: {isFastWrite}");
		if ((ecuType == ECUTYPE.KEIHINOTHER) & isFastWrite)
		{
			SendCommand(array, array.Length, ref reply, ref replySize);
			Console.WriteLine("Response received: " + BitConverter.ToString(reply));
			if (reply[1] == 5 && reply[3] == 0)
			{
				FTDI.FT_STATUS fT_STATUS = FTDI.FT_SetBaudRate(ftHandle, 62500u);
				if (fT_STATUS == FTDI.FT_STATUS.FT_OK)
				{
					Console.WriteLine("Baud rate berhasil diatur ke 62500.");
					Thread.Sleep(100);
				}
				else
				{
					Console.WriteLine($"Gagal mengatur baud rate! FT_STATUS: {fT_STATUS}");
				}
			}
			else
			{
				Console.WriteLine("Respons tidak valid atau tidak memenuhi kondisi.");
			}
		}
		else
		{
			switch (ecuType)
			{
			case ECUTYPE.KEIHINH8C:
				num2 = 64;
				num3 = 4;
				num7 = binSize / 64;
				array14[0] = 126;
				array14[1] = 75;
				array14[2] = 1;
				array14[3] = 6;
				flag = false;
				break;
			case ECUTYPE.KEIHINM32R:
				flag = false;
				break;
			case ECUTYPE.SHINDENGENV850:
				array14[0] = 124;
				array14[1] = 139;
				array14[2] = 2;
				array14[3] = 6;
				num6 = startOffset / num2;
				if (!isFastWrite)
				{
					sleepMs = 150;
				}
				break;
			case ECUTYPE.SHINDENGENRH850:
				array14[0] = 124;
				array14[1] = 139;
				array14[2] = 2;
				array14[3] = 6;
				num6 = startOffset / num2;
				flag = false;
				if (!isFastWrite)
				{
					sleepMs = 150;
				}
				break;
			case ECUTYPE.SHINDENGENRH850B:
				array14[0] = 124;
				array14[1] = 139;
				array14[2] = 2;
				array14[3] = 6;
				num6 = startOffset / num2;
				flag = false;
				if (!isFastWrite)
				{
					sleepMs = 150;
				}
				break;
			}
		}
		stopwatch.Start();
		while (num6 < num7)
		{
			array14[4] = (byte)((num4 >> 8) & 0xFF);
			array14[5] = (byte)(num4 & 0xFF);
			Array.Copy(binBytes, num6 * num2, array14, 6, num2);
			if (flag)
			{
				for (int j = 1; j < num7 - num6; j++)
				{
					if (!CompareMemory(binBytes, (num6 + j) * num2, array15, num2))
					{
						num5 = num4 + j * num3;
						num6 += j - 1;
						break;
					}
					if (num6 + j + 1 == num7)
					{
						num6 += j;
					}
				}
			}
			else
			{
				num5 = num4 + num3;
			}
			if (num6 + 1 == num7)
			{
				num5 = 0;
			}
			array14[6 + num2] = (byte)((num5 >> 8) & 0xFF);
			array14[6 + num2 + 1] = (byte)(num5 & 0xFF);
			array14[6 + num2 + 2] = Checksum8Bit(array14, num2 + 4, 4);
			array14[6 + num2 + 3] = Checksum8BitHonda(array14, num2 + 4, 4);
			array14[6 + num2 + 4] = Checksum8BitHonda(array14, array14[1] - 1);
			while (SendCommand(array14, array14[1], ref reply, ref replySize, sleepMs))
			{
				STATUS sTATUS;
				if (reply[1] == 7)
				{
					if (reply[4] == 0 && reply[5] == 0)
					{
						if (num < 2)
						{
							num++;
							continue;
						}
						sTATUS = STATUS.FAILED;
					}
					else
					{
						sTATUS = STATUS.FAILED;
					}
				}
				else if (reply[1] == 5)
				{
					timeSpan = stopwatch.Elapsed;
					if (num2 == 64 && (num6 + 1) % 2 == 0)
					{
						SendCommand(array3, array3[1], ref reply, ref replySize);
						Thread.Sleep(250);
					}
					sTATUS = STATUS.SUCCESS;
				}
				else
				{
					sTATUS = STATUS.FAILED;
				}
				double num8 = (double)(num6 + 1) * (double)num2;
				double num9 = num8 / (double)binSize;
				SetControlText(TxtPb, string.Concat("⏳ " + (num9 * 100.0).ToString("F0") + "%"));
				SetProgressBarPosition(PbProgress, (int)(num9 * 10000.0));
				if (sTATUS != STATUS.FAILED)
				{
					num4 = num5;
					num6++;
					break;
				}
				stopwatch.Stop();
				SetControlText(TxtPartCode, "Writing flash failed!!");
				MessageBox.Show("Write Flash failed\r\nRestart Aplikasi\r\nKemudian Flash Ulang", "Write Flash", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
				SetControlText(TxtPb, "");
				SetProgressBarPosition(PbProgress, 0);
				return;
			}
		}
		stopwatch.Stop();
		if (ecuType == ECUTYPE.SHINDENGENV850 || ecuType == ECUTYPE.SHINDENGENRH850 || ecuType == ECUTYPE.SHINDENGENRH850B)
		{
			SendCommand(array9, array9.Length, ref reply, ref replySize, sleepMs);
			Thread.Sleep(1000);
			SendCommand(array10, array10.Length, ref reply, ref replySize, sleepMs);
			Thread.Sleep(1000);
			SendCommand(array11, array11.Length, ref reply, ref replySize, sleepMs);
			Thread.Sleep(1000);
			SendCommand(array12, array12.Length, ref reply, ref replySize, sleepMs);
			Thread.Sleep(3000);
			if (SendCommand(array13, array13.Length, ref reply, ref replySize, sleepMs) && reply[3] == 15)
			{
				SystemSounds.Asterisk.Play();
				SetControlText(TxtPartCode, "Writing Flash Complete");
				MessageBox.Show("Write Flash Completed\r\nResult = GOOD\r\nRestart Your ECM", "Write Flash", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
				SetControlText(TxtPb, "");
				SetProgressBarPosition(PbProgress, 0);
				return;
			}
		}
		else
		{
			if (num2 == 64)
			{
				SendCommand(array3, array3.Length, ref reply, ref replySize);
				Thread.Sleep(100);
			}
			SendCommand(array4, array4.Length, ref reply, ref replySize);
			Thread.Sleep(1000);
			SendCommand(array2, array2.Length, ref reply, ref replySize);
			SendCommand(array5, array5.Length, ref reply, ref replySize);
			Thread.Sleep(1000);
			SendCommand(array2, array2.Length, ref reply, ref replySize);
			SendCommand(array6, array6.Length, ref reply, ref replySize);
			Thread.Sleep(1000);
			SendCommand(array2, array2.Length, ref reply, ref replySize);
			SendCommand(array7, array7.Length, ref reply, ref replySize);
			Thread.Sleep(1000);
			if (SendCommand(array2, array2.Length, ref reply, ref replySize) && reply[3] == 15 && SendCommand(array8, array8.Length, ref reply, ref replySize) && reply[3] == 15)
			{
				SystemSounds.Beep.Play();
				SetControlText(TxtPartCode, "Writing Flash Complete");
				MessageBox.Show("Write Flash Completed\r\nResult = GOOD\r\nRestart Your ECM", "Write Flash", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
				SetControlText(TxtPb, "");
				SetProgressBarPosition(PbProgress, 0);
				return;
			}
		}
		MessageBox.Show("Write Flash Completed\r\nResult = BAD!!\r\nPlease Restart the ECU and try to flash it again", "Write Flash", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
		SetControlText(TxtPb, "");
		SetProgressBarPosition(PbProgress, 0);
	}

	private void KeihinAutoChecksum(int checksumOffset = 0)
	{
		byte b = ((checksumOffset != 0) ? ((byte)(Sum8Bit(binBytes, checksumOffset) + Sum8Bit(binBytes, binSize - (checksumOffset + 1), checksumOffset + 1))) : Sum8Bit(binBytes, binSize - 1, 1));
		binBytes[checksumOffset] = (byte)(-b);
	}

	private bool KeihinPreWriteFlash()
	{
		byte[] array = new byte[8] { 125, 8, 1, 2, 80, 71, 77, 148 };
		byte[] array2 = new byte[8] { 125, 8, 1, 3, 45, 70, 73, 187 };
		byte[] array3 = new byte[5] { 126, 5, 1, 2, 122 };
		byte[] array4 = new byte[7] { 126, 7, 1, 3, 0, 0, 119 };
		byte[] array5 = new byte[11]
		{
			126, 11, 1, 11, 0, 0, 0, 255, 255, 255,
			110
		};
		byte[] array6 = new byte[7] { 126, 7, 1, 14, 1, 144, 219 };
		byte[] array7 = new byte[6] { 126, 6, 1, 4, 255, 120 };
		byte[] array8 = new byte[5] { 126, 5, 1, 5, 119 };
		byte[] array9 = new byte[6] { 126, 6, 1, 1, 0, 122 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		int num = 0;
		while (true)
		{
			if (!SendCommand(array, array.Length, ref reply, ref replySize))
			{
				Thread.Sleep(500);
				if (num >= 1)
				{
					break;
				}
				num++;
				continue;
			}
			SendCommand(array2, array2.Length, ref reply, ref replySize);
			break;
		}
		Thread.Sleep(200);
		SendCommand(array9, array9.Length, ref reply, ref replySize);
		for (int i = 0; i < 10; i++)
		{
			SetControlText(TxtPb, "⏳ " + (10 - i).ToString("D"));
			SetProgressBarPosition(PbProgress, 10000 - 1000 * i);
			Thread.Sleep(1000);
			if (i == 9)
			{
				SetProgressBarPosition(PbProgress, 0);
				Thread.Sleep(500);
			}
		}
		SendCommand(array3, array3.Length, ref reply, ref replySize, 250);
		SendCommand(array4, array4.Length, ref reply, ref replySize);
		SendCommand(array9, array9.Length, ref reply, ref replySize);
		SendCommand(array5, array5.Length, ref reply, ref replySize);
		SendCommand(array9, array9.Length, ref reply, ref replySize);
		SendCommand(array6, array6.Length, ref reply, ref replySize);
		if (SendCommand(array7, array7.Length, ref reply, ref replySize) && reply[3] == 0)
		{
			Thread.Sleep(2000);
			if (SendCommand(array8, array8.Length, ref reply, ref replySize, 250) && reply[3] == 0)
			{
				SendCommand(array9, array9.Length, ref reply, ref replySize);
				return true;
			}
			if (reply[3] == 250)
			{
				MessageBox.Show("Delete Failed, Block Error", "Write Flash", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
				return false;
			}
		}
		MessageBox.Show("Failed to Delete", "Write Flash", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
		return false;
	}

	private bool ValidateChecksum(byte[] data, int dataSize)
	{
		return Checksum8BitHonda(data, dataSize - 1) == data[dataSize - 1];
	}

	private bool CompareMemory(byte[] data, int dataIndex, byte[] pattern, int compareLength)
	{
		bool result;
		if (dataIndex + compareLength > data.Length)
		{
			result = false;
		}
		else
		{
			for (int i = 0; i < compareLength; i++)
			{
				if (data[dataIndex + i] != pattern[i])
				{
					return false;
				}
			}
			result = true;
		}
		return result;
	}

	private uint Sum32Bit(byte[] data, int dataSize, int startPos = 0)
	{
		int num = startPos + dataSize;
		uint num2 = 0u;
		while (startPos + 4 <= num)
		{
			num2 += BitConverter.ToUInt32(data, startPos);
			startPos += 4;
		}
		return num2;
	}

	private void SetProgressBarPosition(ProgressBar progressBar, int position)
	{
		if (progressBar.InvokeRequired)
		{
			SetProgressBarPositionCallBack method = SetProgressBarPosition;
			progressBar.Invoke(method, progressBar, position);
		}
		else
		{
			progressBar.Value = position;
		}
	}

	private bool GetShindengenRecoveryKey(byte[] seedByte, ref byte[] key)
	{
		for (int i = 0; i < binSize - shRKeysSig.Length; i++)
		{
			if (CompareMemory(binBytes, i + 6, shRKeysSig, shRKeysSig.Length))
			{
				ushort num = (ushort)(seedByte[0] * 256 + seedByte[1]);
				ushort num2 = (ushort)(binBytes[i + 1] * 256 + binBytes[i]);
				ushort num3 = (ushort)(binBytes[i + 3] * 256 + binBytes[i + 2]);
				ushort num4 = (ushort)(binBytes[i + 5] * 256 + binBytes[i + 4]);
				ushort num5 = (ushort)(num3 * (num + num2));
				bool result;
				if (num4 > 0)
				{
					key[0] = (byte)((num5 + num % num4 >> 8) & 0xFF);
					key[1] = (byte)((num5 + num % num4) & 0xFF);
					result = true;
				}
				else
				{
					key[0] = (byte)((num5 + num >> 8) & 0xFF);
					key[1] = (byte)((num5 + num) & 0xFF);
					result = true;
				}
				return result;
			}
		}
		return false;
	}

	private bool GetShindengenWriteKey(byte[] seedByte, ref byte[] key)
	{
		for (int i = 0; i < binSize - shWKeysig.Length; i++)
		{
			if (!CompareMemory(binBytes, i, shWKeysig, shWKeysig.Length))
			{
				continue;
			}
			for (int j = i + shWKeysig.Length; j < binSize - 10; j++)
			{
				if (binBytes[j + 8] == 238 && (binBytes[j + 9] == 0 || binBytes[j + 9] == byte.MaxValue))
				{
					ushort num = (ushort)(seedByte[0] * 256 + seedByte[1]);
					ushort num2 = (ushort)(binBytes[j + 1] * 256 + binBytes[j]);
					ushort num3 = (ushort)(binBytes[j + 3] * 256 + binBytes[j + 2]);
					ushort num4 = (ushort)(binBytes[j + 5] * 256 + binBytes[j + 4]);
					ushort num5 = (ushort)(num3 * (num + num2));
					if (num4 > 0)
					{
						key[0] = (byte)((num5 + num % num4 >> 8) & 0xFF);
						key[1] = (byte)((num5 + num % num4) & 0xFF);
						return true;
					}
					key[0] = (byte)((num5 + num >> 8) & 0xFF);
					key[1] = (byte)((num5 + num) & 0xFF);
					return true;
				}
			}
		}
		return false;
	}

	private byte Checksum8Bit(byte[] data, int dataSize, int startPos = 0)
	{
		int num = startPos + dataSize;
		int num2 = 0;
		while (startPos < num)
		{
			num2 += data[startPos];
			startPos++;
		}
		return (byte)(255 - (num2 - 1 >> 8));
	}

	private byte Checksum8BitHonda(byte[] data, int dataSize, int startPos = 0)
	{
		int num = startPos + dataSize;
		int num2 = 0;
		while (startPos < num)
		{
			num2 += data[startPos];
			startPos++;
		}
		return (byte)(((num2 ^ 0xFF) + 1) & 0xFF);
	}

	private byte Sum8Bit(byte[] data, int dataSize, int startPos = 0)
	{
		int num = startPos + dataSize;
		byte b = 0;
		while (startPos < num)
		{
			b += data[startPos];
			startPos++;
		}
		return b;
	}

	private void MainForm_Closing(object sender, FormClosingEventArgs e)
	{
		endThread = true;
	}

	private void BtnWrite_Click(object sender, EventArgs e)
	{
		BtnWrite.Enabled = false;
		button1.Enabled = true;
		button1.Enabled = false;
	}

	private void CbFastWrite_CheckedChanged(object sender, EventArgs e)
	{
		isFastWrite = CbFastWrite.Checked;
	}

	private void ChecksumOffset_Changed(object sender, EventArgs e)
	{
		if (TbChecksumOffset.TextLength > 0 && !TbChecksumOffset.Text.Contains("Auto"))
		{
			try
			{
				checksumOffset = int.Parse(TbChecksumOffset.Text, NumberStyles.HexNumber);
				return;
			}
			catch (FormatException)
			{
				checksumOffset = 0;
				return;
			}
		}
		checksumOffset = 0;
	}

	private void StartOffset_Changed(object sender, EventArgs e)
	{
		if (TbStartOffset.TextLength > 0 && !TbStartOffset.Text.Contains("Auto"))
		{
			try
			{
				startOffset = int.Parse(TbStartOffset.Text, NumberStyles.HexNumber);
				return;
			}
			catch (FormatException)
			{
				startOffset = 0;
				return;
			}
		}
		startOffset = 0;
	}

	private double ConvertByteToValue(byte byteValue)
	{
		return (double)(int)byteValue / 255.0 * 87.85 - 14.7;
	}

	private double ConvertByteToValue2(byte byteValue)
	{
		return (double)(int)byteValue / 255.0 * 87.85 - 14.7 + 14.7;
	}

	private byte ConvertValueToByte(double displayedValue)
	{
		double value = (displayedValue + 14.7) / 87.85 * 255.0;
		return (byte)Clamp(value, 0.0, 255.0);
	}

	private double Clamp(double value, double min, double max)
	{
		if (value < min)
		{
			return min;
		}
		if (value > max)
		{
			return max;
		}
		return value;
	}

	private void MasukkanNilaiBaru(object sender, EventArgs e)
	{
		if (dataGridView1.SelectedCells.Count == 0)
		{
			MessageBox.Show("Pilih setidaknya satu sel terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		if (!double.TryParse(txtInput.Text, out var result))
		{
			MessageBox.Show("Masukkan nilai persentase yang valid.", "Kesalahan Input", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		foreach (DataGridViewCell selectedCell in dataGridView1.SelectedCells)
		{
			selectedCell.Value = result;
		}
	}

	private void KurangiNilai(object sender, EventArgs e)
	{
		if (dataGridView1.SelectedCells.Count == 0)
		{
			MessageBox.Show("Pilih setidaknya satu sel terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		if (!double.TryParse(txtInput.Text, out var result))
		{
			MessageBox.Show("Masukkan nilai persentase yang valid.", "Kesalahan Input", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		foreach (DataGridViewCell selectedCell in dataGridView1.SelectedCells)
		{
			if (double.TryParse(selectedCell.Value?.ToString(), out var result2))
			{
				selectedCell.Value = result2 - result;
			}
		}
	}

	private void TambahNilai(object sender, EventArgs e)
	{
		if (dataGridView1.SelectedCells.Count == 0)
		{
			MessageBox.Show("Pilih setidaknya satu sel terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		if (!double.TryParse(txtInput.Text, out var result))
		{
			MessageBox.Show("Masukkan nilai persentase yang valid.", "Kesalahan Input", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		foreach (DataGridViewCell selectedCell in dataGridView1.SelectedCells)
		{
			try
			{
				if (double.TryParse(selectedCell.Value?.ToString(), out var result2))
				{
					selectedCell.Value = result2 + result;
				}
				else
				{
					selectedCell.Value = result2 - result;
				}
			}
			catch
			{
				if (double.TryParse(selectedCell.Value?.ToString(), out var _))
				{
					selectedCell.Value = result;
				}
			}
		}
	}

	private void KaliNilai(object sender, EventArgs e)
	{
		if (dataGridView1.SelectedCells.Count == 0)
		{
			MessageBox.Show("Pilih setidaknya satu sel terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		if (!double.TryParse(txtInput.Text, out var result))
		{
			MessageBox.Show("Masukkan nilai persentase yang valid.", "Kesalahan Input", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		double num = result / 100.0;
		foreach (DataGridViewCell selectedCell in dataGridView1.SelectedCells)
		{
			if (selectedCell.Value != null && double.TryParse(selectedCell.Value.ToString(), out var result2))
			{
				double value = result2 * (1.0 + num);
				selectedCell.Value = Math.Round(value, 3);
			}
			else
			{
				double value2 = 1.0 - num;
				selectedCell.Value = Math.Round(value2, 3);
			}
		}
	}

	private void BagiNilai(object sender, EventArgs e)
	{
		if (dataGridView1.SelectedCells.Count == 0)
		{
			MessageBox.Show("Pilih setidaknya satu sel terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		if (!double.TryParse(txtInput.Text, out var result))
		{
			MessageBox.Show("Masukkan nilai persentase yang valid.", "Kesalahan Input", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		double num = result / 100.0;
		foreach (DataGridViewCell selectedCell in dataGridView1.SelectedCells)
		{
			if (selectedCell.Value != null && double.TryParse(selectedCell.Value.ToString(), out var result2))
			{
				double value = result2 * (1.0 - num);
				selectedCell.Value = Math.Round(value, 3);
			}
		}
	}

	private void TambahNilaiInterpolasi(object sender, EventArgs e)
	{
		double result;
		if (dataGridView1.SelectedCells.Count == 0)
		{
			MessageBox.Show("Pilih setidaknya satu sel terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}
		else if (!double.TryParse(txtInput.Text, out result))
		{
			MessageBox.Show("Masukkan nilai persentase yang valid.", "Kesalahan Input", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		else
		{
			if (dataGridView1.SelectedCells.Count <= 0)
			{
				return;
			}
			double num = result / 100.0;
			List<DataGridViewCell> source = dataGridView1.SelectedCells.Cast<DataGridViewCell>().ToList();
			List<int> list = (from result2 in source.Select((DataGridViewCell cell) => cell.RowIndex).Distinct()
				orderby result2
				select result2).ToList();
			List<int> list2 = (from col in source.Select((DataGridViewCell cell) => cell.ColumnIndex).Distinct()
				orderby col
				select col).ToList();
			foreach (int row in list)
			{
				List<DataGridViewCell> list3 = (from cell in source
					where cell.RowIndex == row
					orderby cell.ColumnIndex
					select cell).ToList();
				if (list3.Count > 1)
				{
					double[] array = list3.Select((DataGridViewCell cell) => double.TryParse(cell.Value?.ToString(), out var result2) ? result2 : 0.0).ToArray();
					for (int num2 = 1; num2 < list3.Count - 1; num2++)
					{
						double num3 = array[num2 - 1];
						double num4 = array[num2 + 1];
						double num5 = num3 + (num4 - num3) * num;
						list3[num2].Value = num5;
					}
					list3[0].Value = array[0];
					list3[list3.Count - 1].Value = array[array.Length - 1];
				}
			}
			foreach (int column in list2)
			{
				List<DataGridViewCell> list4 = (from cell in source
					where cell.ColumnIndex == column
					orderby cell.RowIndex
					select cell).ToList();
				if (list4.Count > 1)
				{
					double[] array2 = list4.Select((DataGridViewCell cell) => double.TryParse(cell.Value?.ToString(), out var result2) ? result2 : 0.0).ToArray();
					for (int num6 = 1; num6 < list4.Count - 1; num6++)
					{
						double num7 = array2[num6 - 1];
						double num8 = array2[num6 + 1];
						double num9 = num7 + (num8 - num7) * num;
						list4[num6].Value = num9;
					}
					list4[0].Value = array2[0];
					list4[list4.Count - 1].Value = array2[array2.Length - 1];
				}
			}
		}
	}

	private void show_all_Click(object sender, EventArgs e)
	{
		Form1 form = new Form1();
		Graph3DMainForm graph3DMainForm = new Graph3DMainForm();
		graph3DMainForm.Show();
	}

	private void isivom()
	{
		comboBox1.Items.Add("1. 30400-K25G-601");
		comboBox1.Items.Add("2. 30400-K81A-N02");
		comboBox1.Items.Add("3. 30400-K81A-N11");
		comboBox1.Items.Add("4. 30400-K81A-N12");
		comboBox1.Items.Add("5. 30400-K81H-N52");
		comboBox1.Items.Add("6. 30400-K81H-N62");
		comboBox1.Items.Add("7. 30400-K1AA-N01");
		comboBox1.Items.Add("8. 30400-K1AA-N02");
		comboBox1.Items.Add("9. 30400-K1AA-N03");
		comboBox1.Items.Add("10. 30400-K1AA-N11");
		comboBox1.Items.Add("11. 30400-K1AA-N12");
		comboBox1.Items.Add("12. 30400-K1AA-N13");
		comboBox1.Items.Add("13. 30400-K60A-B01");
		comboBox1.Items.Add("14. 30400-K60A-B11");
		comboBox1.Items.Add("15. 30400-K60K-B21");
		comboBox1.Items.Add("16. 30400-K60K-B31");
		comboBox1.Items.Add("17. 30400-K60R-B61");
		comboBox1.Items.Add("18. 30400-K60R-B71");
		comboBox1.Items.Add("19. 30400-K59A-A11");
		comboBox1.Items.Add("20. 30400-K59F-A01");
		comboBox1.Items.Add("21. 30400-K59J-A71");
		comboBox1.Items.Add("22. 30400-KZRA-601");
		comboBox1.Items.Add("23. 30400-KZRA-651");
		comboBox1.Items.Add("24. 30400-KZRJ-B11");
		comboBox1.Items.Add("25. 30400-KZRM-B21");
		comboBox1.Items.Add("26. 30400-KZRM-B31");
		comboBox1.Items.Add("27. 30400-K2VG-N42");
		comboBox1.Items.Add("28. 30400-K2VG-N32");
		comboBox1.Items.Add("29. 30400-K2SA-N02");
		comboBox1.Items.Add("30. 30400-KOWA-N01");
		comboBox1.Items.Add("31. 30400-K1AL-NB1");
		comboBox1.Items.Add("32. 30400-K1AL-N91");
		comboBox1.Items.Add("33. 30400-K97G-N01");
		comboBox1.Items.Add("34. 30400-K97G-N31");
		comboBox1.Items.Add("35. 30400-K1ZG-N21");
		comboBox1.Items.Add("36. 30400-K1ZG-N31");
		comboBox1.Items.Add("37. 30400-K1ZN-N41");
		comboBox1.Items.Add("38. 30400-K1ZN-N51");
		comboBox1.Items.Add("39. 30400-KOWL-NA1");
		comboBox1.Items.Add("40. 30400-KOWL-NB1-IN01");
		comboBox1.Items.Add("41. 30400-KOWL-NB1-IN11");
		comboBox1.Items.Add("42. 30400-K0WA-N11");
		comboBox1.Items.Add("43. 38770-K18-902");
		comboBox1.Items.Add("44. 38770-k15-903");
		comboBox1.Items.Add("45. 38770-K15-921");
		comboBox1.Items.Add("46. 38770-K15-601");
		comboBox1.Items.Add("47. 38770-K45-N01");
		comboBox1.Items.Add("48. 38770-K45-N11");
		comboBox1.Items.Add("49. 38770-K45-N42");
		comboBox1.Items.Add("50. 38770-K45-NA1");
		comboBox1.Items.Add("51. 38770-K45-NL1-PP1");
		comboBox1.Items.Add("52. 38770-K56-N01");
		comboBox1.Items.Add("53. 38770-K56-NJ1");
		comboBox1.Items.Add("54. 38770-K56-N11");
		comboBox1.Items.Add("55. 38770-K84-901");
		comboBox1.Items.Add("56. 38770-K18G-941");
		comboBox1.Items.Add("57. 38770-K18-941");
		comboBox1.Items.Add("58. 38770-K03-N32");
		comboBox1.Items.Add("59. 38770-K46-N01");
		comboBox1.Items.Add("60. 38770-K25-901");
		comboBox1.Items.Add("61. 38770-K41-N01");
		comboBox1.Items.Add("62. 38770-K18-961");
		comboBox1.Items.Add("63. 30400-K93-N01");
		comboBox1.Items.Add("64. 30400-KOJ-N01");
		comboBox1.Items.Add("65. 30400-KOJ-N11");
		comboBox1.Items.Add("66. 30400-KOJ-N61");
		comboBox1.Items.Add("67. 30400-KOJ-N71");
		comboBox1.Items.Add("68. 30400-K2F-N01");
		comboBox1.Items.Add("69. 30400-K2F-N11");
		comboBox1.Items.Add("70. 30400-K2F-N81");
		comboBox1.Items.Add("71. 30400-K2F-N91");
		comboBox1.Items.Add("72. 30400-K46-N21");
		comboBox1.Items.Add("73. 30400-K46-NA1");
		comboBox1.Items.Add("74. 30400-K46-NB1");
		comboBox1.Items.Add("75. 38770-KZL-C31");
		comboBox1.Items.Add("76. 30400-K3VA-NO1");
		comboBox1.Items.Add("77. 30400-K25G-611");
		comboBox1.Items.Add("78. 38770-K38-N01");
		comboBox1.Items.Add("79. 38770-K41-NC1");
		comboBox1.Items.Add("80. 38770-KYZ-713");
		comboBox1.Items.Add("81. 38770-K15-711");
		comboBox1.Items.Add("82. 30400-K1AL-N81-IN11");
		comboBox1.Items.Add("83. 30400-K1AL-N81-IN01");
		comboBox1.Items.Add("84. 38770-KYE-942");
		comboBox1.Items.Add("85. 30400-K0WM-TA1");
		comboBox1.Items.Add("86. 30400-K0WF-T01");
		comboBox1.Items.Add("87. 30400-K1ZF-T01");
		comboBox1.Items.Add("88. 30400-K1ZF-T11");
		comboBox1.Items.Add("89. 30400-K1ZP-T51");
		comboBox1.Items.Add("90. 30400-K1ZP-T71");
		comboBox1.Items.Add("91. 30400-K36F-T01");
		comboBox1.Items.Add("92. 30400-K36S-T31");
		comboBox1.Items.Add("93. 30400-K97F-T01");
		comboBox1.Items.Add("94. 30400-K2TA-T02");
		comboBox1.Items.Add("95. 30400-K20J-T21");
		comboBox1.Items.Add("96. 38770-K20-901");
		comboBox1.Items.Add("97. 38770-K20-T01");
		comboBox1.Items.Add("98. 30400-K2F-T21");
		comboBox1.Items.Add("99. 30400-K2F-T81");
		comboBox1.Items.Add("100. 30400-K2F-T91");
		comboBox1.Items.Add("101. 30400-K93-T01");
		comboBox1.Items.Add("102. 30400-K93-T21");
		comboBox1.Items.Add("103. 30400-K2SF-T01");
		comboBox1.Items.Add("104. 30400-K60F-T01");
		comboBox1.Items.Add("105. 30400-K2VH-T11");
		comboBox1.Items.Add("106. 30400-K3MF-T01");
		comboBox1.Items.Add("107. 30400-K59K-T11");
		comboBox1.Items.Add("108. 30400-K60S-M61");
		comboBox1.Items.Add("109. 30400-K60S-T73");
		comboBox1.Items.Add("110. 38770-K58-TC1");
		comboBox1.Items.Add("111. 38770-K58-TC2");
		comboBox1.Items.Add("112. 38770-K58-N41");
		comboBox1.Items.Add("113. 38770-K58-T81");
		comboBox1.Items.Add("114. 38770-K2J-TH1");
		comboBox1.Items.Add("115. 38770-K2J-T01");
		comboBox1.Items.Add("116. 38770-K03-T61");
		comboBox1.Items.Add("117. 38770-K03-H01");
		comboBox1.Items.Add("118. 38770-KWW-643");
		comboBox1.Items.Add("119. 38770-K58-N81");
		comboBox1.Items.Add("120. 38770-K58R-N81");
		comboBox1.Items.Add("121. 38770-KYZ-712");
		comboBox1.Items.Add("122. 38770-K41K-NC1");
		comboBox1.Items.Add("123. 38770-K3F-T01");
		comboBox1.Items.Add("124. 38770-K73-T32");
		comboBox1.Items.Add("125. 38770-K73-TC1");
		comboBox1.Items.Add("126. 38770-KYZ-T41");
		comboBox1.Items.Add("127. 38770-KYZ-T03");
		comboBox1.Items.Add("128. 38770-K76-T62");
		comboBox1.Items.Add("129. 38770-KVB-S52");
		comboBox1.Items.Add("130. 38770-KZL-C01");
		comboBox1.Items.Add("131. 38770-K2E-T11");
		comboBox1.Items.Add("132. 38770-K26-B02");
		comboBox1.Items.Add("133. 38770-K26-921");
		comboBox1.Items.Add("134. 38770-K26-911");
		comboBox1.Items.Add("135. 38770-K26-901");
		comboBox1.Items.Add("136. 38770-K1M-T01");
		comboBox1.Items.Add("137. 38770-K1M-T31");
		comboBox1.Items.Add("138. 30400-K1AL-NA1");
		comboBox1.Items.Add("139. 30400-K35A-T01");
		comboBox1.Items.Add("140. 30400-K35A-V01");
		comboBox1.Items.Add("141. 30400-K97A-U01");
		comboBox1.Items.Add("142. 38770-CAP-H01");
		comboBox1.Items.Add("143. 38770-KPG-H01");
		comboBox1.Items.Add("144. 38770-KPG-H21");
		comboBox1.Items.Add("145. 38770-KPY-H01");
		comboBox1.Items.Add("146. 38770-KPY-H61");
		comboBox1.Items.Add("147. 38770-K46-N31");
		comboBox1.Items.Add("148. 30400-K1ZV-NA1");
		comboBox1.Items.Add("149. 30400-K1ZV-NC1");
		comboBox1.Items.Add("150. 30400-K1ZV-NF1");
		comboBox1.Items.Add("151. 30400-K2F-NC1");
		comboBox1.Items.Add("152. 30400-K2F-ND1");
		comboBox1.Items.Add("153. 30400-KOJ-NA1");
		comboBox1.Items.Add("154. 30400-KOJ-NB1");
		comboBox1.Items.Add("155. 30400-K2VM-N81");
		comboBox1.Items.Add("156. 30400-K2VM-NA1");
		comboBox1.Items.Add("157. 30400-K2VM-V51");
		comboBox1.Items.Add("158. 30400-KOWW-NC2");
		comboBox1.Items.Add("159. 30400-KOWW-ND2");
		comboBox1.Items.Add("160. 30400-KOWW-NE2");
		if (comboBox1.Items.Count > 0)
		{
			comboBox1.SelectedIndex = 0;
		}
	}

	private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
	{
		string text = comboBox1.SelectedItem.ToString();
		string text2 = null;
		customBigEndian = false;
		if (customProfiles.ContainsKey(text))
		{
			text2 = ApplyCustomProfile(text);
		}
		else
		switch (text)
		{
		case "1. 30400-K25G-601":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 107988;
			rpmh = 108038;
			iaph = 0;
			feul1 = 102704;
			feul2 = 104154;
			feul3 = 0;
			ig1 = 108125;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			limit1 = 101976;
			limit2 = 102170;
			limit3 = 102474;
			limit4 = 107126;
			limit5 = 106872;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			kon = 1f;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K25G-601.bin";
			break;
		case "2. 30400-K81A-N02":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108024;
			rpmh = 108074;
			iaph = 0;
			feul1 = 102736;
			feul2 = 104186;
			feul3 = 0;
			ig1 = 108161;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			limit1 = 102202;
			limit2 = 102506;
			limit3 = 107158;
			limit4 = 102008;
			limit5 = 105844;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 101472;
			o2c = 109062;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			kon = 1f;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.BEAT 110-30400-K81A-N02.bin";
			break;
		case "3. 30400-K81A-N11":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108024;
			rpmh = 108074;
			iaph = 0;
			feul1 = 102736;
			feul2 = 104186;
			feul3 = 0;
			ig1 = 108161;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			limit1 = 102202;
			limit2 = 102506;
			limit3 = 107158;
			limit4 = 102008;
			limit5 = 105844;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 101472;
			o2c = 109062;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			kon = 1f;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.BEAT 110-30400-K81A-N11.bin";
			break;
		case "4. 30400-K81A-N12":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108024;
			rpmh = 108074;
			iaph = 0;
			feul1 = 102736;
			feul2 = 104186;
			feul3 = 0;
			ig1 = 108161;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			limit1 = 102202;
			limit2 = 102506;
			limit3 = 107158;
			limit4 = 102008;
			limit5 = 105844;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 101472;
			o2c = 109062;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			kon = 1f;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.BEAT 110- 30400-K81A-N12.bin";
			break;
		case "5. 30400-K81H-N52":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108024;
			rpmh = 108074;
			iaph = 0;
			feul1 = 102736;
			feul2 = 104186;
			feul3 = 0;
			ig1 = 108161;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			limit1 = 102202;
			limit2 = 102506;
			limit3 = 107158;
			limit4 = 102008;
			limit5 = 105844;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 101472;
			o2c = 109062;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			kon = 1f;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.BEAT -- 30400-K81H-N52.bin";
			break;
		case "6. 30400-K81H-N62":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108024;
			rpmh = 108074;
			iaph = 0;
			feul1 = 102736;
			feul2 = 104186;
			feul3 = 0;
			ig1 = 108161;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			limit1 = 102202;
			limit2 = 102506;
			limit3 = 107158;
			limit4 = 102008;
			limit5 = 105844;
			limit6 = 0;
			limit7 = 0;
			o2 = 101472;
			o2c = 109062;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			kon = 1f;
			lm = 1f;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.BEAT - K81H-N62.bin";
			break;
		case "7. 30400-K1AA-N01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 35;
			tpsh = 236542;
			rpmh = 236600;
			iaph = 0;
			feul1 = 229726;
			feul2 = 231756;
			feul3 = 240942;
			ig1 = 236705;
			ig2 = 237720;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235608;
			limit2 = 229122;
			limit3 = 229496;
			limit4 = 228888;
			limit5 = 233998;
			limit6 = 234008;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.BEAT KIA-30400-N01.bin";
			break;
		case "8. 30400-K1AA-N02":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 35;
			tpsh = 236542;
			rpmh = 236600;
			iaph = 0;
			feul1 = 229726;
			feul2 = 231756;
			feul3 = 240942;
			ig1 = 236705;
			ig2 = 237720;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235608;
			limit2 = 229122;
			limit3 = 229496;
			limit4 = 228888;
			limit5 = 233998;
			limit6 = 234008;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.BEAT - K1AA-N02.bin";
			break;
		case "9. 30400-K1AA-N03":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 35;
			tpsh = 236542;
			rpmh = 236600;
			iaph = 0;
			feul1 = 229726;
			feul2 = 231756;
			feul3 = 240942;
			ig1 = 236705;
			ig2 = 237720;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235608;
			limit2 = 229122;
			limit3 = 229496;
			limit4 = 228888;
			limit5 = 233998;
			limit6 = 234008;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.BEAT -  2020-K1AA-N03.bin";
			break;
		case "10. 30400-K1AA-N11":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 35;
			tpsh = 236542;
			rpmh = 236600;
			iaph = 0;
			feul1 = 229726;
			feul2 = 231756;
			feul3 = 240942;
			ig1 = 236705;
			ig2 = 237720;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235608;
			limit2 = 229122;
			limit3 = 229496;
			limit4 = 228888;
			limit5 = 233998;
			limit6 = 234008;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.BEAT KIA-30400-N11.bin";
			break;
		case "11. 30400-K1AA-N12":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 35;
			tpsh = 236542;
			rpmh = 236600;
			iaph = 0;
			feul1 = 229726;
			feul2 = 231756;
			feul3 = 240942;
			ig1 = 236705;
			ig2 = 237720;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235608;
			limit2 = 229122;
			limit3 = 229496;
			limit4 = 228888;
			limit5 = 233998;
			limit6 = 234008;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.BEAT KIA-30400-N12.bin";
			break;
		case "12. 30400-K1AA-N13":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 35;
			tpsh = 236542;
			rpmh = 236600;
			iaph = 0;
			feul1 = 229726;
			feul2 = 231756;
			feul3 = 240942;
			ig1 = 236705;
			ig2 = 237720;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235608;
			limit2 = 229122;
			limit3 = 229496;
			limit4 = 228888;
			limit5 = 233998;
			limit6 = 234008;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.BEAT - 2020 K1AA-N13.bin";
			break;
		case "13. 30400-K60A-B01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 107916;
			rpmh = 107966;
			iaph = 0;
			feul1 = 102636;
			feul2 = 104086;
			feul3 = 0;
			ig1 = 108053;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102102;
			limit2 = 102406;
			limit3 = 107058;
			limit4 = 101908;
			limit5 = 105736;
			limit6 = 105744;
			limit7 = 0;
			lm = 1f;
			o2 = 101404;
			o2c = 108944;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.VARIO 125 - K60A-B01-ORI.bin";
			break;
		case "14. 30400-K60A-B11":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 107916;
			rpmh = 107966;
			iaph = 0;
			feul1 = 102636;
			feul2 = 104086;
			feul3 = 0;
			ig1 = 108053;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102102;
			limit2 = 102406;
			limit3 = 107058;
			limit4 = 101908;
			limit5 = 105736;
			limit6 = 105744;
			limit7 = 0;
			lm = 1f;
			o2 = 101404;
			o2c = 108944;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.VARIO 125 -- 30400-K60A-B11.bin";
			break;
		case "15. 30400-K60K-B21":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108016;
			rpmh = 108066;
			iaph = 0;
			feul1 = 102732;
			feul2 = 104182;
			feul3 = 0;
			ig1 = 108153;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102004;
			limit2 = 102198;
			limit3 = 102502;
			limit4 = 107154;
			limit5 = 105832;
			limit6 = 105840;
			limit7 = 0;
			lm = 1f;
			o2 = 101500;
			o2c = 109044;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.VARI 125 - K60K-B21.bin";
			break;
		case "16. 30400-K60K-B31":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108016;
			rpmh = 108066;
			iaph = 0;
			feul1 = 102732;
			feul2 = 104182;
			feul3 = 0;
			ig1 = 108153;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102004;
			limit2 = 102198;
			limit3 = 102502;
			limit4 = 107154;
			limit5 = 105832;
			limit6 = 105840;
			limit7 = 0;
			lm = 1f;
			o2 = 101500;
			o2c = 109044;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.VARIO 125 - K60K-B31.bin";
			break;
		case "17. 30400-K60R-B61":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108240;
			rpmh = 108290;
			iaph = 0;
			feul1 = 110564;
			feul2 = 102920;
			feul3 = 104370;
			ig1 = 108377;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102386;
			limit2 = 102690;
			limit3 = 107342;
			limit4 = 102192;
			limit5 = 106020;
			limit6 = 106028;
			limit7 = 0;
			lm = 1f;
			o2 = 115256;
			o2c = 109270;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.VARIO 125 - K60R-B61.bin";
			break;
		case "18. 30400-K60R-B71":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108240;
			rpmh = 108290;
			iaph = 0;
			feul1 = 110564;
			feul2 = 102920;
			feul3 = 104370;
			ig1 = 108377;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102386;
			limit2 = 102690;
			limit3 = 107342;
			limit4 = 102192;
			limit5 = 106020;
			limit6 = 106028;
			limit7 = 0;
			lm = 1f;
			o2 = 115256;
			o2c = 109270;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.VARIO 125 K60R-B71.bin";
			break;
		case "19. 30400-K59A-A11":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 107916;
			rpmh = 107966;
			iaph = 0;
			feul1 = 102636;
			feul2 = 104086;
			feul3 = 0;
			ig1 = 108053;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102102;
			limit2 = 102406;
			limit3 = 107058;
			limit4 = 101908;
			limit5 = 105736;
			limit6 = 105744;
			limit7 = 0;
			lm = 1f;
			o2 = 101404;
			o2c = 108944;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.VARIO 150 K59A-A11-RAT.bin";
			break;
		case "20. 30400-K59F-A01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108016;
			rpmh = 108066;
			iaph = 0;
			feul1 = 102732;
			feul2 = 104182;
			feul3 = 0;
			ig1 = 108153;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102004;
			limit2 = 102198;
			limit3 = 102502;
			limit4 = 107154;
			limit5 = 105832;
			limit6 = 105840;
			limit7 = 0;
			lm = 1f;
			o2 = 101500;
			o2c = 109044;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.VARIO 150 K59F A01 IN 01.bin";
			break;
		case "21. 30400-K59J-A71":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108240;
			rpmh = 108290;
			iaph = 0;
			feul1 = 110564;
			feul2 = 102920;
			feul3 = 104370;
			ig1 = 108377;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102386;
			limit2 = 102690;
			limit3 = 107342;
			limit4 = 102192;
			limit5 = 106020;
			limit6 = 106028;
			limit7 = 0;
			lm = 1f;
			o2 = 115256;
			o2c = 109270;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 109299;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.VARIO 150-K59J-A71-IN-01.bin";
			break;
		case "22. 30400-KZRA-601":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108640;
			rpmh = 108690;
			iaph = 0;
			feul1 = 103616;
			feul2 = 105066;
			feul3 = 0;
			ig1 = 108777;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102976;
			limit2 = 103080;
			limit3 = 103386;
			limit4 = 106716;
			limit5 = 107782;
			limit6 = 107682;
			limit7 = 0;
			lm = 1f;
			o2 = 102608;
			o2c = 109652;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.601read.bin";
			break;
		case "23. 30400-KZRA-651":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108640;
			rpmh = 108690;
			iaph = 0;
			feul1 = 103616;
			feul2 = 105066;
			feul3 = 0;
			ig1 = 108777;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102976;
			limit2 = 103080;
			limit3 = 103386;
			limit4 = 106716;
			limit5 = 107782;
			limit6 = 107682;
			limit7 = 0;
			lm = 1f;
			o2 = 102608;
			o2c = 109652;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.651READ.bin";
			break;
		case "24. 30400-KZRJ-B11":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108640;
			rpmh = 108690;
			iaph = 0;
			feul1 = 103616;
			feul2 = 105066;
			feul3 = 0;
			ig1 = 108777;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102976;
			limit2 = 103080;
			limit3 = 103386;
			limit4 = 106716;
			limit5 = 107782;
			limit6 = 107682;
			limit7 = 0;
			lm = 1f;
			o2 = 102608;
			o2c = 109652;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.kzrb11.bin";
			break;
		case "25. 30400-KZRM-B21":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108640;
			rpmh = 108690;
			iaph = 0;
			feul1 = 103616;
			feul2 = 105066;
			feul3 = 0;
			ig1 = 108777;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102976;
			limit2 = 103080;
			limit3 = 103386;
			limit4 = 106716;
			limit5 = 107782;
			limit6 = 107682;
			limit7 = 0;
			lm = 1f;
			o2 = 102608;
			o2c = 109652;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.kzrb21.bin";
			break;
		case "26. 30400-KZRM-B31":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108640;
			rpmh = 108690;
			iaph = 0;
			feul1 = 103616;
			feul2 = 105066;
			feul3 = 0;
			ig1 = 108777;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102976;
			limit2 = 103080;
			limit3 = 103386;
			limit4 = 106716;
			limit5 = 107782;
			limit6 = 107682;
			limit7 = 0;
			lm = 1f;
			o2 = 102608;
			o2c = 109652;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.kzrb31.bin";
			break;
		case "27. 30400-K2VG-N42":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = true;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = true;
			toolStripMenuItem20.Visible = true;
			toolStripMenuItem21.Visible = true;
			toolStripMenuItem22.Visible = true;
			offMode4ToolStripMenuItem.Visible = true;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 243100;
			rpmh = 243158;
			iaph = 0;
			feul1 = 235658;
			feul2 = 237688;
			feul3 = 247850;
			ig1 = 243263;
			ig2 = 244278;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 234740;
			limit2 = 234974;
			limit3 = 235428;
			limit4 = 239930;
			limit5 = 241732;
			limit6 = 239940;
			limit7 = 241626;
			lm = 1f;
			o2 = 0;
			o2c = 253768;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 246021;
			immomask = 1;
			MODE0 = 245977;
			MODE1 = 245978;
			MODE2 = 245980;
			MODE3 = 245993;
			MODE4 = 245996;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.VARIO 125-30400-K2VG-N42 ORIGINAL.bin";
			break;
		case "28. 30400-K2VG-N32":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = true;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = true;
			toolStripMenuItem20.Visible = true;
			toolStripMenuItem21.Visible = true;
			toolStripMenuItem22.Visible = true;
			offMode4ToolStripMenuItem.Visible = true;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 243100;
			rpmh = 243158;
			iaph = 0;
			feul1 = 235658;
			feul2 = 237688;
			feul3 = 247850;
			ig1 = 243263;
			ig2 = 244278;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 234740;
			limit2 = 234974;
			limit3 = 235428;
			limit4 = 239930;
			limit5 = 241732;
			limit6 = 239940;
			limit7 = 241626;
			lm = 1f;
			o2 = 0;
			o2c = 253768;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 246021;
			immomask = 1;
			MODE0 = 245977;
			MODE1 = 245978;
			MODE2 = 245980;
			MODE3 = 245993;
			MODE4 = 245996;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.VARIO 125-30400-K2VG-N32 ORIGINAL.bin";
			break;
		case "29. 30400-K2SA-N02":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 35;
			tpsh = 242984;
			rpmh = 243042;
			iaph = 0;
			feul1 = 235542;
			feul2 = 237572;
			feul3 = 0;
			ig1 = 243147;
			ig2 = 244162;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 234624;
			limit2 = 239814;
			limit3 = 241518;
			limit4 = 241616;
			limit5 = 234858;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 245905;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.VARIO160-30400-K2SA-N02.bin";
			break;
		case "30. 30400-KOWA-N01":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 104252;
			rpmh = 104302;
			iaph = 104227;
			feul1 = 106122;
			feul2 = 111460;
			feul3 = 0;
			iap = 104372;
			ig1 = 107907;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 100906;
			limit2 = 101356;
			limit3 = 103188;
			limit4 = 100634;
			limit5 = 101722;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 109127;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.adv150_kowa_n01.bin";
			break;
		case "31. 30400-K1AL-NB1":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = true;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 35;
			tpsh = 243822;
			rpmh = 243880;
			iaph = 0;
			feul1 = 236320;
			feul2 = 238350;
			feul3 = 248566;
			iap = 0;
			ig1 = 243985;
			ig2 = 245000;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235400;
			limit2 = 235634;
			limit3 = 236088;
			limit4 = 240592;
			limit5 = 240602;
			limit6 = 242394;
			limit7 = 242288;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 246741;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K1AL-NB1 KYLES.bin";
			break;
		case "32. 30400-K1AL-N91":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = true;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 35;
			tpsh = 243822;
			rpmh = 243880;
			iaph = 0;
			feul1 = 236320;
			feul2 = 238350;
			feul3 = 248566;
			iap = 0;
			ig1 = 243985;
			ig2 = 245000;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235400;
			limit2 = 235634;
			limit3 = 236088;
			limit4 = 240592;
			limit5 = 240602;
			limit6 = 242394;
			limit7 = 242288;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 109127;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.BEAT NEW-30400-K1AL-N91 ISS.bin";
			break;
		case "33. 30400-K97G-N01":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 103988;
			rpmh = 104038;
			iaph = 103963;
			feul1 = 105858;
			feul2 = 0;
			feul3 = 0;
			iap = 104108;
			ig1 = 107643;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 100866;
			limit2 = 101236;
			limit3 = 102924;
			limit4 = 100618;
			limit5 = 101602;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 108864;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.PCX 150 - K97G-N01.bin";
			break;
		case "34. 30400-K97G-N31":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 104004;
			rpmh = 104054;
			iaph = 103979;
			feul1 = 105874;
			feul2 = 0;
			feul3 = 0;
			iap = 104124;
			ig1 = 107659;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 100882;
			limit2 = 101252;
			limit3 = 102940;
			limit4 = 100634;
			limit5 = 101618;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 108864;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			lm = 1f;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.PCX 150 - K97G-N31.bin";
			break;
		case "35. 30400-K1ZG-N21":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 247940;
			rpmh = 247990;
			iaph = 247915;
			feul1 = 249810;
			feul2 = 256804;
			feul3 = 0;
			iap = 248060;
			ig1 = 251595;
			ig2 = 252470;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 243608;
			limit2 = 246222;
			limit3 = 244060;
			limit4 = 243340;
			limit5 = 244426;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 254137;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.PCX 160-K1ZG-N 21.bin";
			break;
		case "36. 30400-K1ZG-N31":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 247940;
			rpmh = 247990;
			iaph = 247915;
			feul1 = 249810;
			feul2 = 256804;
			feul3 = 0;
			iap = 248060;
			ig1 = 251595;
			ig2 = 252470;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 243608;
			limit2 = 246222;
			limit3 = 244060;
			limit4 = 243340;
			limit5 = 244426;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 254137;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.pcx_160_k1zg_n31.bin";
			break;
		case "37. 30400-K1ZN-N41":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = true;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 248604;
			rpmh = 248654;
			iaph = 248579;
			feul1 = 250474;
			feul2 = 257480;
			feul3 = 0;
			iap = 248724;
			ig1 = 252259;
			ig2 = 253134;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 244004;
			limit2 = 244272;
			limit3 = 245090;
			limit4 = 245100;
			limit5 = 246616;
			limit6 = 246886;
			limit7 = 244724;
			lm = 1f;
			o2 = 109093;
			o2c = 266900;
			o2mask = 42;
			o2cmask = 33;
			IMMO = 254801;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.PCX 160-30400-K1ZN-N41 ORIGINAL.bin";
			break;
		case "38. 30400-K1ZN-N51":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = true;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 248604;
			rpmh = 248654;
			iaph = 248579;
			feul1 = 250474;
			feul2 = 257480;
			feul3 = 0;
			iap = 248724;
			ig1 = 252259;
			ig2 = 253134;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 244004;
			limit2 = 244272;
			limit3 = 245090;
			limit4 = 245100;
			limit5 = 246616;
			limit6 = 246886;
			limit7 = 244724;
			lm = 1f;
			o2 = 109093;
			o2c = 266900;
			o2mask = 42;
			o2cmask = 33;
			IMMO = 254801;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.PCX 160-30400-K1ZN-N51 ORIGINAL.bin";
			break;
		case "39. 30400-KOWL-NA1":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = true;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 248604;
			rpmh = 248654;
			iaph = 248579;
			feul1 = 250474;
			feul2 = 257480;
			feul3 = 0;
			iap = 248724;
			ig1 = 252259;
			ig2 = 253134;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 244004;
			limit2 = 244272;
			limit3 = 245090;
			limit4 = 245100;
			limit5 = 246616;
			limit6 = 246886;
			limit7 = 244724;
			lm = 1f;
			o2 = 109093;
			o2c = 266900;
			o2mask = 42;
			o2cmask = 33;
			IMMO = 254801;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.ADV 160-30400-KOWL-NA1.bin";
			break;
		case "40. 30400-KOWL-NB1-IN01":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = true;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 248604;
			rpmh = 248654;
			iaph = 248579;
			feul1 = 250474;
			feul2 = 257480;
			feul3 = 0;
			iap = 248724;
			ig1 = 252259;
			ig2 = 253134;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 244004;
			limit2 = 244272;
			limit3 = 245090;
			limit4 = 245100;
			limit5 = 246616;
			limit6 = 246886;
			limit7 = 244724;
			lm = 1f;
			o2 = 109093;
			o2c = 266900;
			o2mask = 42;
			o2cmask = 33;
			IMMO = 254801;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.ADV 160-30400-KOWL-NB1-IN01.bin";
			break;
		case "41. 30400-KOWL-NB1-IN11":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = true;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 248604;
			rpmh = 248654;
			iaph = 248579;
			feul1 = 250474;
			feul2 = 257480;
			feul3 = 0;
			iap = 248724;
			ig1 = 252259;
			ig2 = 253134;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 244004;
			limit2 = 244272;
			limit3 = 245090;
			limit4 = 245100;
			limit5 = 246616;
			limit6 = 246886;
			limit7 = 244724;
			lm = 1f;
			o2 = 109093;
			o2c = 266900;
			o2mask = 42;
			o2cmask = 33;
			IMMO = 254801;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.ADV 160-30400-KOWL-NB1-IN11.bin";
			break;
		case "42. 30400-K0WA-N11":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 104252;
			rpmh = 104302;
			iaph = 104227;
			feul1 = 106122;
			feul2 = 111460;
			feul3 = 0;
			iap = 104372;
			ig1 = 107907;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 100906;
			limit2 = 101356;
			limit3 = 103188;
			limit4 = 100634;
			limit5 = 101722;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 109127;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.ADV 150-KOWA-N11 IMMO OFF.bin";
			break;
		case "43. 38770-K18-902":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 30282;
			rpmh = 30224;
			iaph = 0;
			feul1 = 31782;
			feul2 = 30332;
			feul3 = 0;
			iap = 0;
			ig1 = 33232;
			ig2 = 33957;
			ig3 = 0;
			ig4 = 0;
			kon = 3.6408696f;
			limit1 = 34752;
			limit2 = 9260;
			limit3 = 21032;
			limit4 = 8433;
			limit5 = 3456;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 39492;
			o2mask = 9;
			bass = 39498;
			bassmask = 14;
			ect = 39487;
			ectmask = 4;
			iat = 39489;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.1_VERZA-38770-K18-902_BIN.bin";
			break;
		case "44. 38770-k15-903":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = false;
			toolStripMenuItem13.Visible = false;
			toolStripMenuItem14.Visible = false;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 22;
			rowCount = 29;
			tpsh = 30283;
			rpmh = 30225;
			iaph = 30327;
			feul1 = 31625;
			feul2 = 0;
			feul3 = 0;
			iap = 30349;
			ig1 = 33539;
			ig2 = 32901;
			ig3 = 0;
			ig4 = 0;
			kon = 3.6408696f;
			limit1 = 7342;
			limit2 = 3182;
			limit3 = 0;
			limit4 = 0;
			limit5 = 0;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 38319;
			o2mask = 9;
			bass = 38325;
			bassmask = 14;
			ect = 38314;
			ectmask = 4;
			iat = 38316;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.2_CB150R-38770-k15-903_BIN.bin";
			break;
		case "45. 38770-K15-921":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = true;
			ignition4ToolStripMenuItem.Visible = true;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = false;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = true;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem1.Visible = true;
			columnCount = 29;
			rowCount = 29;
			tpsh = 75;
			rpmh = 17;
			iaph = 133;
			feul1 = 1844;
			feul2 = 0;
			feul3 = 0;
			iap = 162;
			ig1 = 3526;
			ig2 = 4367;
			ig3 = 5208;
			ig4 = 6049;
			kon = 1f;
			limit1 = 7011;
			limit2 = 20382;
			limit3 = 38787;
			limit4 = 46933;
			limit5 = 0;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 13736;
			o2mask = 9;
			bass = 13742;
			bassmask = 14;
			iacv = 13728;
			iacvmask = 1;
			ect = 13731;
			ectmask = 4;
			iat = 13733;
			iatmask = 6;
			vss = 13735;
			vssmask = 8;
			mapp = 13753;
			mappmask = 12;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.3_CB150R-38770-K15-921-BIN.bin";
			break;
		case "46. 38770-K15-601":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = true;
			ignition4ToolStripMenuItem.Visible = true;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = true;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem1.Visible = true;
			columnCount = 29;
			rowCount = 29;
			tpsh = 75;
			rpmh = 17;
			iaph = 133;
			feul1 = 1844;
			feul2 = 0;
			feul3 = 0;
			iap = 162;
			ig1 = 3526;
			ig2 = 4367;
			ig3 = 5208;
			ig4 = 6049;
			kon = 1f;
			limit1 = 7861;
			limit2 = 20290;
			limit3 = 38788;
			limit4 = 47015;
			limit5 = 47466;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 14828;
			o2mask = 9;
			bass = 14834;
			bassmask = 14;
			iacv = 14820;
			iacvmask = 1;
			ect = 14823;
			ectmask = 4;
			iat = 14825;
			iatmask = 6;
			vss = 14827;
			vssmask = 8;
			mapp = 14836;
			mappmask = 12;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.4_CB150R-38770-K15M-601_BIN.bin";
			break;
		case "47. 38770-K45-N01":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = true;
			ignition4ToolStripMenuItem.Visible = true;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = true;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem1.Visible = true;
			columnCount = 29;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 132;
			feul1 = 1843;
			feul2 = 0;
			feul3 = 0;
			iap = 161;
			ig1 = 3525;
			ig2 = 4366;
			ig3 = 5207;
			ig4 = 6048;
			kon = 1f;
			limit1 = 7010;
			limit2 = 20382;
			limit3 = 38627;
			limit4 = 43901;
			limit5 = 46523;
			limit6 = 46744;
			limit7 = 0;
			lm = 1f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 12859;
			o2mask = 9;
			bass = 12865;
			bassmask = 14;
			iacv = 12851;
			iacvmask = 1;
			ect = 12854;
			ectmask = 4;
			iat = 12856;
			iatmask = 6;
			vss = 12858;
			vssmask = 8;
			mapp = 13753;
			mappmask = 12;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.cbr150_38770_k45_n01.bin";
			break;
		case "48. 38770-K45-N11":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = true;
			ignition4ToolStripMenuItem.Visible = true;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = true;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem1.Visible = true;
			columnCount = 29;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 132;
			feul1 = 1843;
			feul2 = 0;
			feul3 = 0;
			iap = 161;
			ig1 = 3525;
			ig2 = 4366;
			ig3 = 5207;
			ig4 = 6048;
			kon = 1f;
			limit1 = 7010;
			limit2 = 20382;
			limit3 = 38627;
			limit4 = 43901;
			limit5 = 46523;
			limit6 = 46744;
			limit7 = 0;
			lm = 1f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 12859;
			o2mask = 9;
			bass = 12865;
			bassmask = 14;
			iacv = 12851;
			iacvmask = 1;
			ect = 12854;
			ectmask = 4;
			iat = 12856;
			iatmask = 6;
			vss = 12858;
			vssmask = 8;
			mapp = 13753;
			mappmask = 12;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.cbr150_38770_k45_n11.bin";
			break;
		case "49. 38770-K45-N42":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = true;
			ignition4ToolStripMenuItem.Visible = true;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = true;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem1.Visible = true;
			columnCount = 29;
			rowCount = 29;
			tpsh = 75;
			rpmh = 17;
			iaph = 133;
			feul1 = 1844;
			feul2 = 0;
			feul3 = 0;
			iap = 162;
			ig1 = 3526;
			ig2 = 4367;
			ig3 = 5208;
			ig4 = 6049;
			kon = 1f;
			limit1 = 7861;
			limit2 = 20282;
			limit3 = 47429;
			limit4 = 38758;
			limit5 = 46978;
			limit6 = 47036;
			limit7 = 0;
			lm = 1f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 14570;
			o2mask = 9;
			bass = 14576;
			bassmask = 14;
			iacv = 14562;
			iacvmask = 1;
			ect = 14565;
			ectmask = 4;
			iat = 14567;
			iatmask = 6;
			vss = 14569;
			vssmask = 8;
			mapp = 14587;
			mappmask = 12;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.6_CBR150R-38770-K45G-N42-BIN.bin";
			break;
		case "50. 38770-K45-NA1":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = true;
			ignition4ToolStripMenuItem.Visible = true;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = true;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem1.Visible = true;
			columnCount = 29;
			rowCount = 29;
			tpsh = 75;
			rpmh = 17;
			iaph = 133;
			feul1 = 1844;
			feul2 = 0;
			feul3 = 0;
			iap = 162;
			ig1 = 3526;
			ig2 = 4367;
			ig3 = 5208;
			ig4 = 6049;
			kon = 1f;
			limit1 = 7861;
			limit2 = 20290;
			limit3 = 47493;
			limit4 = 47042;
			limit5 = 38824;
			limit6 = 47100;
			limit7 = 0;
			lm = 1f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 14829;
			o2mask = 9;
			bass = 14835;
			bassmask = 14;
			iacv = 14821;
			iacvmask = 1;
			ect = 14824;
			ectmask = 4;
			iat = 14826;
			iatmask = 6;
			vss = 14828;
			vssmask = 8;
			mapp = 14837;
			mappmask = 12;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.7_CBR150R-38770-K45G-NA1_BIN.bin";
			break;
		case "51. 38770-K45-NL1-PP1":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = true;
			ignition4ToolStripMenuItem.Visible = true;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = true;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem1.Visible = true;
			columnCount = 29;
			rowCount = 29;
			tpsh = 75;
			rpmh = 17;
			iaph = 133;
			feul1 = 1844;
			feul2 = 0;
			feul3 = 0;
			iap = 162;
			ig1 = 3526;
			ig2 = 4367;
			ig3 = 5208;
			ig4 = 6049;
			kon = 1f;
			limit1 = 7861;
			limit2 = 20290;
			limit3 = 47493;
			limit4 = 47042;
			limit5 = 38824;
			limit6 = 47100;
			limit7 = 0;
			lm = 1f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 14829;
			o2mask = 9;
			bass = 14835;
			bassmask = 14;
			iacv = 14821;
			iacvmask = 1;
			ect = 14824;
			ectmask = 4;
			iat = 14826;
			iatmask = 6;
			vss = 14828;
			vssmask = 8;
			mapp = 14837;
			mappmask = 12;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.CBR 150 R-38770-K45-NL1-PP1.bin";
			break;
		case "52. 38770-K56-N01":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = true;
			ignition4ToolStripMenuItem.Visible = true;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = false;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = true;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem1.Visible = true;
			columnCount = 29;
			rowCount = 29;
			tpsh = 75;
			rpmh = 17;
			iaph = 133;
			feul1 = 1844;
			feul2 = 0;
			feul3 = 0;
			iap = 162;
			ig1 = 3526;
			ig2 = 4367;
			ig3 = 5208;
			ig4 = 6049;
			kon = 1f;
			limit1 = 7011;
			limit2 = 20382;
			limit3 = 38787;
			limit4 = 46933;
			limit5 = 0;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 13736;
			o2mask = 9;
			bass = 13742;
			bassmask = 14;
			iacv = 13728;
			iacvmask = 1;
			ect = 13731;
			ectmask = 4;
			iat = 13733;
			iatmask = 6;
			vss = 13735;
			vssmask = 8;
			mapp = 13753;
			mappmask = 12;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.k56 sonic.bin";
			break;
		case "53. 38770-K56-NJ1":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = true;
			ignition4ToolStripMenuItem.Visible = true;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = true;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem1.Visible = true;
			columnCount = 29;
			rowCount = 29;
			tpsh = 75;
			rpmh = 17;
			iaph = 133;
			feul1 = 1844;
			feul2 = 0;
			feul3 = 0;
			iap = 162;
			ig1 = 3526;
			ig2 = 4367;
			ig3 = 5208;
			ig4 = 6049;
			kon = 1f;
			limit1 = 7861;
			limit2 = 20290;
			limit3 = 47493;
			limit4 = 47042;
			limit5 = 38824;
			limit6 = 47100;
			limit7 = 0;
			lm = 1f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 14829;
			o2mask = 9;
			bass = 14835;
			bassmask = 14;
			iacv = 14821;
			iacvmask = 1;
			ect = 14824;
			ectmask = 4;
			iat = 14826;
			iatmask = 6;
			vss = 14828;
			vssmask = 8;
			mapp = 14837;
			mappmask = 12;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K56-NJ1.bin";
			break;
		case "54. 38770-K56-N11":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = true;
			ignition4ToolStripMenuItem.Visible = true;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = false;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = true;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem1.Visible = true;
			columnCount = 29;
			rowCount = 29;
			tpsh = 75;
			rpmh = 17;
			iaph = 133;
			feul1 = 1844;
			feul2 = 0;
			feul3 = 0;
			iap = 162;
			ig1 = 3526;
			ig2 = 4367;
			ig3 = 5208;
			ig4 = 6049;
			kon = 1f;
			limit1 = 7011;
			limit2 = 20382;
			limit3 = 38787;
			limit4 = 46933;
			limit5 = 0;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 13736;
			o2mask = 9;
			bass = 13742;
			bassmask = 14;
			iacv = 13728;
			iacvmask = 1;
			ect = 13731;
			ectmask = 4;
			iat = 13733;
			iatmask = 6;
			vss = 13735;
			vssmask = 8;
			mapp = 13753;
			mappmask = 12;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.9_SUPRAGTR-38770-K56-N11-BIN.bin";
			break;
		case "55. 38770-K84-901":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = true;
			ignition4ToolStripMenuItem.Visible = true;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = true;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem1.Visible = true;
			columnCount = 29;
			rowCount = 29;
			tpsh = 75;
			rpmh = 17;
			iaph = 133;
			feul1 = 1844;
			feul2 = 0;
			feul3 = 0;
			iap = 162;
			ig1 = 3526;
			ig2 = 4367;
			ig3 = 5208;
			ig4 = 6049;
			kon = 1f;
			limit1 = 7861;
			limit2 = 20290;
			limit3 = 47493;
			limit4 = 47042;
			limit5 = 13436;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 14829;
			o2mask = 9;
			bass = 14835;
			bassmask = 14;
			iacv = 14821;
			iacvmask = 1;
			ect = 14824;
			ectmask = 4;
			iat = 14826;
			iatmask = 6;
			vss = 14828;
			vssmask = 8;
			mapp = 14837;
			mappmask = 12;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.STD.bin";
			break;
		case "56. 38770-K18G-941":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 4548;
			limit2 = 16074;
			limit3 = 45122;
			limit4 = 38646;
			limit5 = 44295;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 9766;
			o2mask = 9;
			bass = 9772;
			bassmask = 14;
			ect = 9761;
			ectmask = 4;
			iat = 9763;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.12_VERZA-38770-K18G-941-BIN.bin";
			break;
		case "57. 38770-K18-941":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 4548;
			limit2 = 16074;
			limit3 = 45122;
			limit4 = 38646;
			limit5 = 44295;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 9766;
			o2mask = 9;
			bass = 9772;
			bassmask = 14;
			ect = 9761;
			ectmask = 4;
			iat = 9763;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.CB150-K18-941.bin";
			break;
		case "58. 38770-K03-N32":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 4548;
			limit2 = 16048;
			limit3 = 38530;
			limit4 = 44179;
			limit5 = 45006;
			limit6 = 4858;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 9741;
			o2mask = 9;
			bass = 9747;
			bassmask = 14;
			ect = 9736;
			ectmask = 4;
			iat = 9738;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.13_REVO-38770-K03-N32-BIN.bin";
			break;
		case "59. 38770-K46-N01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 4548;
			limit2 = 16048;
			limit3 = 38530;
			limit4 = 44179;
			limit5 = 45006;
			limit6 = 4858;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 9741;
			o2mask = 9;
			bass = 9747;
			bassmask = 14;
			ect = 9736;
			ectmask = 4;
			iat = 9738;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.14_VARIO110_BEAT110-38770-K46-N01.bin";
			break;
		case "60. 38770-K25-901":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 30282;
			rpmh = 30224;
			iaph = 0;
			feul1 = 31782;
			feul2 = 30332;
			feul3 = 0;
			iap = 0;
			ig1 = 33232;
			ig2 = 33957;
			ig3 = 0;
			ig4 = 0;
			kon = 3.6408696f;
			limit1 = 34752;
			limit2 = 9260;
			limit3 = 21032;
			limit4 = 8433;
			limit5 = 3456;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 39492;
			o2mask = 9;
			bass = 39498;
			bassmask = 14;
			ect = 39487;
			ectmask = 4;
			iat = 39489;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.15_BEAT110-38770-K25-901-BIN.bin";
			break;
		case "61. 38770-K41-N01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 4548;
			limit2 = 16048;
			limit3 = 38530;
			limit4 = 44179;
			limit5 = 45006;
			limit6 = 4858;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 9741;
			o2mask = 9;
			bass = 9747;
			bassmask = 14;
			ect = 9736;
			ectmask = 4;
			iat = 9738;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.17_BLADE-38770-K41-N01-BIN.bin";
			break;
		case "62. 38770-K18-961":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 5273;
			limit2 = 15959;
			limit3 = 45219;
			limit4 = 38628;
			limit5 = 44386;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 10935;
			o2mask = 9;
			bass = 10941;
			bassmask = 14;
			ect = 10930;
			ectmask = 4;
			iat = 10932;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.verza_38770_k18_961.bin";
			break;
		case "63. 30400-K93-N01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = false;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 238056;
			rpmh = 238108;
			iaph = 0;
			feul1 = 239620;
			feul2 = 238168;
			feul3 = 0;
			iap = 0;
			ig1 = 244144;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235344;
			limit2 = 235684;
			limit3 = 235836;
			limit4 = 236008;
			limit5 = 0;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.STD32.bin";
			break;
		case "64. 30400-KOJ-N01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = false;
			toolStripMenuItem14.Visible = false;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 203930;
			rpmh = 203980;
			iaph = 0;
			feul1 = 204038;
			feul2 = 205488;
			feul3 = 0;
			iap = 0;
			ig1 = 210634;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 203554;
			limit2 = 207132;
			limit3 = 208952;
			limit4 = 0;
			limit5 = 0;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.GENIO 110-30400-KOJ-N01 ORIGINAL.bin";
			break;
		case "65. 30400-KOJ-N11":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = false;
			toolStripMenuItem14.Visible = false;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 203930;
			rpmh = 203980;
			iaph = 0;
			feul1 = 204038;
			feul2 = 205488;
			feul3 = 0;
			iap = 0;
			ig1 = 210634;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 203554;
			limit2 = 207132;
			limit3 = 208952;
			limit4 = 0;
			limit5 = 0;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.GENIO 110-30400-KOJ-N11 ORIGINAL.bin";
			break;
		case "66. 30400-KOJ-N61":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 212390;
			rpmh = 212440;
			iaph = 0;
			feul1 = 212498;
			feul2 = 213948;
			feul3 = 0;
			iap = 0;
			ig1 = 219134;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 212014;
			limit2 = 215592;
			limit3 = 217412;
			limit4 = 217480;
			limit5 = 218110;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.GENIO 110-30400-KOJ-N61 ORIGINAL.bin";
			break;
		case "67. 30400-KOJ-N71":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 212390;
			rpmh = 212440;
			iaph = 0;
			feul1 = 212498;
			feul2 = 213948;
			feul3 = 0;
			iap = 0;
			ig1 = 219134;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 212014;
			limit2 = 215592;
			limit3 = 217412;
			limit4 = 217480;
			limit5 = 218110;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.GENIO 110-30400-KOJ-N71 ORIGINAL.bin";
			break;
		case "68. 30400-K2F-N01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 212390;
			rpmh = 212440;
			iaph = 0;
			feul1 = 212498;
			feul2 = 213948;
			feul3 = 0;
			iap = 0;
			ig1 = 219134;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 212014;
			limit2 = 215592;
			limit3 = 217412;
			limit4 = 217480;
			limit5 = 218110;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.SCOOPY-30400-K2F-N01.bin";
			break;
		case "69. 30400-K2F-N11":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 212390;
			rpmh = 212440;
			iaph = 0;
			feul1 = 212498;
			feul2 = 213948;
			feul3 = 0;
			iap = 0;
			ig1 = 219134;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 212014;
			limit2 = 215592;
			limit3 = 217412;
			limit4 = 217480;
			limit5 = 218110;
			limit6 = 217564;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 224449;
			immomask = 64;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.SCOOPY-30400-K2F-N11.bin";
			break;
		case "70. 30400-K2F-N81":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 212390;
			rpmh = 212440;
			iaph = 0;
			feul1 = 212498;
			feul2 = 213948;
			feul3 = 0;
			iap = 0;
			ig1 = 219134;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 212014;
			limit2 = 215592;
			limit3 = 217412;
			limit4 = 217480;
			limit5 = 218110;
			limit6 = 217564;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.SCOOPY-30400-K2F-N81.bin";
			break;
		case "71. 30400-K2F-N91":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 212390;
			rpmh = 212440;
			iaph = 0;
			feul1 = 212498;
			feul2 = 213948;
			feul3 = 0;
			iap = 0;
			ig1 = 219134;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 212014;
			limit2 = 215592;
			limit3 = 217412;
			limit4 = 217480;
			limit5 = 218110;
			limit6 = 217564;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 224449;
			immomask = 64;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.SCOOPY-30400-K2F-N91.bin";
			break;
		case "72. 30400-K46-N21":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 233644;
			rpmh = 233696;
			iaph = 0;
			feul1 = 233756;
			feul2 = 235208;
			feul3 = 0;
			iap = 0;
			ig1 = 238572;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 230932;
			limit2 = 231272;
			limit3 = 231424;
			limit4 = 231596;
			limit5 = 237002;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 224449;
			immomask = 64;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.k46n21.bin";
			break;
		case "73. 30400-K46-NA1":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 238056;
			rpmh = 238108;
			iaph = 0;
			feul1 = 239620;
			feul2 = 238168;
			feul3 = 0;
			iap = 0;
			ig1 = 244144;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235344;
			limit2 = 235836;
			limit3 = 236008;
			limit4 = 241414;
			limit5 = 235684;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 64;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.vario 110 2018-30400-k46-na1.bin";
			break;
		case "74. 30400-K46-NB1":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 238056;
			rpmh = 238108;
			iaph = 0;
			feul1 = 239620;
			feul2 = 238168;
			feul3 = 0;
			iap = 0;
			ig1 = 244144;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235344;
			limit2 = 235836;
			limit3 = 236008;
			limit4 = 241414;
			limit5 = 235684;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 64;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.VARIO 110-30400-K46KNB1.bin";
			break;
		case "75. 38770-KZL-C31":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 30282;
			rpmh = 30224;
			iaph = 0;
			feul1 = 31782;
			feul2 = 30332;
			feul3 = 0;
			iap = 0;
			ig1 = 33232;
			ig2 = 33957;
			ig3 = 0;
			ig4 = 0;
			kon = 3.6408696f;
			limit1 = 34752;
			limit2 = 9260;
			limit3 = 21032;
			limit4 = 8433;
			limit5 = 3456;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.SPACY.bin";
			break;
		case "76. 30400-K3VA-NO1":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 35;
			tpsh = 243868;
			rpmh = 243926;
			iaph = 0;
			feul1 = 236426;
			feul2 = 238456;
			feul3 = 0;
			iap = 0;
			ig1 = 244031;
			ig2 = 245046;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235508;
			limit2 = 235742;
			limit3 = 242500;
			limit4 = 242402;
			limit5 = 240698;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 246789;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.STYLO K3VA-N01 ORIGINAL.bin";
			break;
		case "77. 30400-K25G-611":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 107988;
			rpmh = 108038;
			iaph = 0;
			feul1 = 102704;
			feul2 = 104154;
			feul3 = 0;
			iap = 0;
			ig1 = 108125;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102170;
			limit2 = 102474;
			limit3 = 107126;
			limit4 = 101976;
			limit5 = 106872;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K25G-611.bin";
			break;
		case "78. 38770-K38-N01":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = true;
			ignition4ToolStripMenuItem.Visible = true;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 75;
			rpmh = 17;
			iaph = 133;
			feul1 = 1844;
			feul2 = 0;
			feul3 = 0;
			iap = 162;
			ig1 = 3526;
			ig2 = 4367;
			ig3 = 5208;
			ig4 = 6049;
			kon = 1f;
			limit1 = 7861;
			limit2 = 20290;
			limit3 = 38878;
			limit4 = 47105;
			limit5 = 47556;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.cbx150.bin";
			break;
		case "79. 38770-K41-NC1":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 1814;
			feul2 = 132;
			feul3 = 0;
			iap = 0;
			ig1 = 3496;
			ig2 = 4337;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 44395;
			limit2 = 38628;
			limit3 = 45228;
			limit4 = 6467;
			limit5 = 15959;
			limit6 = 6093;
			limit7 = 0;
			lm = 3.64086f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.SUPRA X 125 38770-K41-NC1.bin";
			break;
		case "80. 38770-KYZ-713":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = false;
			toolStripMenuItem13.Visible = false;
			toolStripMenuItem14.Visible = false;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 30282;
			rpmh = 30224;
			iaph = 0;
			feul1 = 31782;
			feul2 = 30332;
			feul3 = 0;
			iap = 0;
			ig1 = 33232;
			ig2 = 33957;
			ig3 = 0;
			ig4 = 0;
			kon = 3.6408696f;
			limit1 = 3456;
			limit2 = 8215;
			limit3 = 0;
			limit4 = 0;
			limit5 = 0;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.SUPRA X 125 HELM IN 38770-KYZ-713.bin";
			break;
		case "81. 38770-K15-711":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = true;
			ignition4ToolStripMenuItem.Visible = true;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 75;
			rpmh = 17;
			iaph = 133;
			feul1 = 1844;
			feul2 = 0;
			feul3 = 0;
			iap = 162;
			ig1 = 3526;
			ig2 = 4367;
			ig3 = 5208;
			ig4 = 6049;
			kon = 1f;
			limit1 = 7861;
			limit2 = 20290;
			limit3 = 47105;
			limit4 = 47556;
			limit5 = 38878;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38779-K15-711.bin";
			break;
		case "82. 30400-K1AL-N81-IN11":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = true;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 35;
			tpsh = 243822;
			rpmh = 243880;
			iaph = 0;
			feul1 = 236320;
			feul2 = 238350;
			feul3 = 248566;
			iap = 0;
			ig1 = 243985;
			ig2 = 245000;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235400;
			limit2 = 235634;
			limit3 = 236088;
			limit4 = 240592;
			limit5 = 240602;
			limit6 = 242394;
			limit7 = 242288;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 109127;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K1AL-N81..bin";
			break;
		case "83. 30400-K1AL-N81-IN01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = true;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 35;
			tpsh = 243822;
			rpmh = 243880;
			iaph = 0;
			feul1 = 236320;
			feul2 = 238350;
			feul3 = 248566;
			iap = 0;
			ig1 = 243985;
			ig2 = 245000;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235400;
			limit2 = 235634;
			limit3 = 236088;
			limit4 = 240592;
			limit5 = 240602;
			limit6 = 242394;
			limit7 = 242288;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 109127;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.KIAL N81 IN01.bin";
			break;
		case "84. 38770-KYE-942":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 30794;
			rpmh = 30736;
			iaph = 0;
			feul1 = 30844;
			feul2 = 32294;
			feul3 = 0;
			iap = 0;
			ig1 = 33744;
			ig2 = 34469;
			ig3 = 0;
			ig4 = 0;
			kon = 3.6408696f;
			limit1 = 10320;
			limit2 = 22286;
			limit3 = 35268;
			limit4 = 9493;
			limit5 = 3588;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 30731;
			o2mask = 32;
			bass = 40471;
			bassmask = 14;
			ect = 40460;
			ectmask = 4;
			iat = 40462;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-KYE-942-ORIGINAL.bin";
			break;
		case "85. 30400-K0WM-TA1":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = true;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 248604;
			rpmh = 248654;
			iaph = 248579;
			feul1 = 250474;
			feul2 = 257480;
			feul3 = 0;
			iap = 248724;
			ig1 = 252259;
			ig2 = 253134;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 244004;
			limit2 = 244272;
			limit3 = 245090;
			limit4 = 245100;
			limit5 = 246616;
			limit6 = 246886;
			limit7 = 244724;
			lm = 1f;
			o2 = 109093;
			o2c = 266900;
			o2mask = 42;
			o2cmask = 33;
			IMMO = 254801;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.ADV 160 THAILAN 30400-K0WM-TA1.bin";
			break;
		case "86. 30400-K0WF-T01":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 104252;
			rpmh = 104302;
			iaph = 104227;
			feul1 = 106122;
			feul2 = 111460;
			feul3 = 0;
			iap = 104372;
			ig1 = 107907;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 100906;
			limit2 = 101356;
			limit3 = 103188;
			limit4 = 100634;
			limit5 = 101722;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 109127;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.ADV 150-30400-K0WF-T01.bin";
			break;
		case "87. 30400-K1ZF-T01":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 247940;
			rpmh = 247990;
			iaph = 247915;
			feul1 = 249810;
			feul2 = 256804;
			feul3 = 0;
			iap = 248060;
			ig1 = 251595;
			ig2 = 252470;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 243608;
			limit2 = 246222;
			limit3 = 244060;
			limit4 = 243340;
			limit5 = 244426;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 254137;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K1ZF-T01.bin";
			break;
		case "88. 30400-K1ZF-T11":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 247940;
			rpmh = 247990;
			iaph = 247915;
			feul1 = 249810;
			feul2 = 256804;
			feul3 = 0;
			iap = 248060;
			ig1 = 251595;
			ig2 = 252470;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 243608;
			limit2 = 246222;
			limit3 = 244060;
			limit4 = 243340;
			limit5 = 244426;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 254137;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.PCX 160 - 30400-K1ZF-T11.bin";
			break;
		case "89. 30400-K1ZP-T51":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = true;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 248604;
			rpmh = 248654;
			iaph = 248579;
			feul1 = 250474;
			feul2 = 257480;
			feul3 = 0;
			iap = 248724;
			ig1 = 252259;
			ig2 = 253134;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 244004;
			limit2 = 244272;
			limit3 = 245090;
			limit4 = 245100;
			limit5 = 246616;
			limit6 = 246886;
			limit7 = 244724;
			lm = 1f;
			o2 = 109093;
			o2c = 266900;
			o2mask = 42;
			o2cmask = 33;
			IMMO = 254801;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K1ZP-T51.bin";
			break;
		case "90. 30400-K1ZP-T71":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = true;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 248604;
			rpmh = 248654;
			iaph = 248579;
			feul1 = 250474;
			feul2 = 257480;
			feul3 = 0;
			iap = 248724;
			ig1 = 252259;
			ig2 = 253134;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 244004;
			limit2 = 244272;
			limit3 = 245090;
			limit4 = 245100;
			limit5 = 246616;
			limit6 = 246886;
			limit7 = 244724;
			lm = 1f;
			o2 = 109093;
			o2c = 266900;
			o2mask = 42;
			o2cmask = 33;
			IMMO = 254801;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K1ZP-T71.bin";
			break;
		case "91. 30400-K36F-T01":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = false;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 103238;
			rpmh = 103288;
			iaph = 103212;
			feul1 = 104796;
			feul2 = 0;
			feul3 = 0;
			iap = 103346;
			ig1 = 106275;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 100094;
			limit2 = 100322;
			limit3 = 102380;
			limit4 = 100692;
			limit5 = 0;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 108864;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K36F-T01.bin";
			break;
		case "92. 30400-K36S-T31":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 103610;
			rpmh = 103660;
			iaph = 103585;
			feul1 = 105480;
			feul2 = 0;
			feul3 = 0;
			iap = 103730;
			ig1 = 107265;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 101064;
			limit2 = 100464;
			limit3 = 100692;
			limit4 = 101430;
			limit5 = 102752;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 108864;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K36S-T31.bin";
			break;
		case "93. 30400-K97F-T01":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 103988;
			rpmh = 104038;
			iaph = 103963;
			feul1 = 105858;
			feul2 = 0;
			feul3 = 0;
			iap = 104108;
			ig1 = 107643;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 100866;
			limit2 = 101236;
			limit3 = 102924;
			limit4 = 100618;
			limit5 = 101602;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 108864;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K97F-T01.bin";
			break;
		case "94. 30400-K2TA-T02":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 35;
			tpsh = 242952;
			rpmh = 243010;
			iaph = 0;
			feul1 = 235510;
			feul2 = 237540;
			feul3 = 0;
			iap = 0;
			ig1 = 243115;
			ig2 = 244130;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 234826;
			limit2 = 241584;
			limit3 = 235280;
			limit4 = 239782;
			limit5 = 234592;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 245873;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K2TA-T02.bin";
			break;
		}
		switch (text)
		{
		case "95. 30400-K20J-T21":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 107988;
			rpmh = 108038;
			iaph = 0;
			feul1 = 102704;
			feul2 = 104154;
			feul3 = 0;
			ig1 = 108125;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			limit1 = 101976;
			limit2 = 102170;
			limit3 = 102474;
			limit4 = 107126;
			limit5 = 106872;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			kon = 1f;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K20J-T21.bin";
			break;
		case "96. 38770-K20-901":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 30282;
			rpmh = 30224;
			iaph = 0;
			feul1 = 31782;
			feul2 = 30332;
			feul3 = 0;
			iap = 0;
			ig1 = 33232;
			ig2 = 33957;
			ig3 = 0;
			ig4 = 0;
			kon = 3.6408696f;
			limit1 = 34752;
			limit2 = 9260;
			limit3 = 21032;
			limit4 = 8433;
			limit5 = 3456;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 39492;
			o2mask = 9;
			bass = 39498;
			bassmask = 14;
			ect = 39487;
			ectmask = 4;
			iat = 39489;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K20-901 Vendor- Keihin -0101270D01-48.bin";
			break;
		case "97. 38770-K20-T01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 4548;
			limit2 = 16048;
			limit3 = 38530;
			limit4 = 44179;
			limit5 = 45006;
			limit6 = 4858;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 9741;
			o2mask = 9;
			bass = 9747;
			bassmask = 14;
			ect = 9736;
			ectmask = 4;
			iat = 9738;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K20-T01 Vendor- Keihin-0101C60D01-64.bin";
			break;
		case "98. 30400-K2F-T21":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 212390;
			rpmh = 212440;
			iaph = 0;
			feul1 = 212498;
			feul2 = 213948;
			feul3 = 0;
			iap = 0;
			ig1 = 219134;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 212014;
			limit2 = 215592;
			limit3 = 217412;
			limit4 = 217480;
			limit5 = 218110;
			limit6 = 217564;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 224449;
			immomask = 64;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K2F-T21-0104140D01.bin";
			break;
		case "99. 30400-K2F-T81":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 212390;
			rpmh = 212440;
			iaph = 0;
			feul1 = 212498;
			feul2 = 213948;
			feul3 = 0;
			iap = 0;
			ig1 = 219134;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 212014;
			limit2 = 215592;
			limit3 = 217412;
			limit4 = 217480;
			limit5 = 218110;
			limit6 = 217564;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 224449;
			immomask = 64;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K2F-T81.bin";
			break;
		case "100. 30400-K2F-T91":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 212390;
			rpmh = 212440;
			iaph = 0;
			feul1 = 212498;
			feul2 = 213948;
			feul3 = 0;
			iap = 0;
			ig1 = 219134;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 212014;
			limit2 = 215592;
			limit3 = 217412;
			limit4 = 217480;
			limit5 = 218110;
			limit6 = 217564;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 224449;
			immomask = 64;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K2F-T91.bin";
			break;
		case "101. 30400-K93-T01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = false;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 238056;
			rpmh = 238108;
			iaph = 0;
			feul1 = 239620;
			feul2 = 238168;
			feul3 = 0;
			iap = 0;
			ig1 = 244144;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235344;
			limit2 = 235684;
			limit3 = 235836;
			limit4 = 236008;
			limit5 = 0;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K93-T01.bin";
			break;
		case "102. 30400-K93-T21":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = false;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 238056;
			rpmh = 238108;
			iaph = 0;
			feul1 = 239620;
			feul2 = 238168;
			feul3 = 0;
			iap = 0;
			ig1 = 244144;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235344;
			limit2 = 235684;
			limit3 = 235836;
			limit4 = 236008;
			limit5 = 0;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K93-T21.bin";
			break;
		case "103. 30400-K2SF-T01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 35;
			tpsh = 242984;
			rpmh = 243042;
			iaph = 0;
			feul1 = 235542;
			feul2 = 237572;
			feul3 = 0;
			ig1 = 243147;
			ig2 = 244162;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 234624;
			limit2 = 239814;
			limit3 = 241518;
			limit4 = 241616;
			limit5 = 234858;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 245905;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K2SF-T01.bin";
			break;
		case "104. 30400-K60F-T01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 107916;
			rpmh = 107966;
			iaph = 0;
			feul1 = 102636;
			feul2 = 104086;
			feul3 = 0;
			ig1 = 108053;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102102;
			limit2 = 102406;
			limit3 = 107058;
			limit4 = 101908;
			limit5 = 105736;
			limit6 = 105744;
			limit7 = 0;
			lm = 1f;
			o2 = 101404;
			o2c = 108944;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K60F-T01.bin";
			break;
		case "105. 30400-K2VH-T11":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = true;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = true;
			toolStripMenuItem20.Visible = true;
			toolStripMenuItem21.Visible = true;
			toolStripMenuItem22.Visible = true;
			offMode4ToolStripMenuItem.Visible = true;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 35;
			tpsh = 243100;
			rpmh = 243158;
			iaph = 0;
			feul1 = 235658;
			feul2 = 237688;
			feul3 = 247850;
			ig1 = 243263;
			ig2 = 244278;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 234740;
			limit2 = 234974;
			limit3 = 235428;
			limit4 = 239930;
			limit5 = 241732;
			limit6 = 239940;
			limit7 = 241626;
			lm = 1f;
			o2 = 0;
			o2c = 253768;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 246021;
			immomask = 1;
			MODE0 = 245977;
			MODE1 = 245978;
			MODE2 = 245980;
			MODE3 = 245993;
			MODE4 = 245996;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K2VH-T11.bin";
			break;
		case "106. 30400-K3MF-T01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = true;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = true;
			toolStripMenuItem20.Visible = true;
			toolStripMenuItem21.Visible = true;
			toolStripMenuItem22.Visible = true;
			offMode4ToolStripMenuItem.Visible = true;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 35;
			tpsh = 243100;
			rpmh = 243158;
			iaph = 0;
			feul1 = 235658;
			feul2 = 237688;
			feul3 = 247850;
			ig1 = 243263;
			ig2 = 244278;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 234740;
			limit2 = 234974;
			limit3 = 235428;
			limit4 = 239930;
			limit5 = 241732;
			limit6 = 239940;
			limit7 = 241626;
			lm = 1f;
			o2 = 0;
			o2c = 253768;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 246021;
			immomask = 1;
			MODE0 = 245977;
			MODE1 = 245978;
			MODE2 = 245980;
			MODE3 = 245993;
			MODE4 = 245996;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.HONDA GIORNO 30400-K3MF-T01.bin";
			break;
		case "107. 30400-K59K-T11":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108240;
			rpmh = 108290;
			iaph = 0;
			feul1 = 110564;
			feul2 = 102920;
			feul3 = 104370;
			ig1 = 108377;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102386;
			limit2 = 102690;
			limit3 = 107342;
			limit4 = 102192;
			limit5 = 106020;
			limit6 = 106028;
			limit7 = 0;
			lm = 1f;
			o2 = 115256;
			o2c = 109270;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 109299;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K59K-T11.bin";
			break;
		case "108. 30400-K60S-M61":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108240;
			rpmh = 108290;
			iaph = 0;
			feul1 = 110564;
			feul2 = 102920;
			feul3 = 104370;
			ig1 = 108377;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102386;
			limit2 = 102690;
			limit3 = 107342;
			limit4 = 102192;
			limit5 = 106020;
			limit6 = 106028;
			limit7 = 0;
			lm = 1f;
			o2 = 115256;
			o2c = 109270;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K60S-M61.bin";
			break;
		case "109. 30400-K60S-T73":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 108240;
			rpmh = 108290;
			iaph = 0;
			feul1 = 110564;
			feul2 = 102920;
			feul3 = 104370;
			ig1 = 108377;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 102386;
			limit2 = 102690;
			limit3 = 107342;
			limit4 = 102192;
			limit5 = 106020;
			limit6 = 106028;
			limit7 = 0;
			lm = 1f;
			o2 = 115256;
			o2c = 109270;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 0;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K60S-T73.bin";
			break;
		case "110. 38770-K58-TC1":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = false;
			toolStripMenuItem14.Visible = false;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 1814;
			feul2 = 132;
			feul3 = 0;
			iap = 0;
			ig1 = 3496;
			ig2 = 4337;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 44395;
			limit2 = 38628;
			limit3 = 45228;
			limit4 = 0;
			limit5 = 0;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 11755;
			o2mask = 9;
			bass = 11761;
			bassmask = 14;
			ect = 11750;
			ectmask = 4;
			iat = 11752;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K58-TC1.bin";
			break;
		case "111. 38770-K58-TC2":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = false;
			toolStripMenuItem14.Visible = false;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 1814;
			feul2 = 132;
			feul3 = 0;
			iap = 0;
			ig1 = 3496;
			ig2 = 4337;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 44395;
			limit2 = 38628;
			limit3 = 45228;
			limit4 = 0;
			limit5 = 0;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 11755;
			o2mask = 9;
			bass = 11761;
			bassmask = 14;
			ect = 11750;
			ectmask = 4;
			iat = 11752;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K58-TC2.bin";
			break;
		case "112. 38770-K58-N41":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 5273;
			limit2 = 5583;
			limit3 = 15959;
			limit4 = 38646;
			limit5 = 45237;
			limit6 = 44404;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 10373;
			o2mask = 9;
			bass = 10379;
			bassmask = 14;
			ect = 10368;
			ectmask = 4;
			iat = 10370;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K58-N41.bin";
			break;
		case "113. 38770-K58-T81":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 5273;
			limit2 = 5583;
			limit3 = 15959;
			limit4 = 38646;
			limit5 = 45237;
			limit6 = 44404;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 10373;
			o2mask = 9;
			bass = 10379;
			bassmask = 14;
			ect = 10368;
			ectmask = 4;
			iat = 10370;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K58-T81-0102B90D01.bin";
			break;
		case "114. 38770-K2J-TH1":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 132;
			feul2 = 1814;
			feul3 = 0;
			iap = 0;
			ig1 = 3496;
			ig2 = 4337;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 6125;
			limit2 = 45374;
			limit3 = 15959;
			limit4 = 38734;
			limit5 = 44541;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 9736;
			o2mask = 90;
			bass = 9738;
			bassmask = 91;
			ect = 9747;
			ectmask = 110;
			iat = 9741;
			iatmask = 98;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K2J-TH1.bin";
			break;
		case "115. 38770-K2J-T01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 132;
			feul2 = 1814;
			feul3 = 0;
			iap = 0;
			ig1 = 3496;
			ig2 = 4337;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 6125;
			limit2 = 45374;
			limit3 = 15959;
			limit4 = 38734;
			limit5 = 44541;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 9736;
			o2mask = 90;
			bass = 9738;
			bassmask = 91;
			ect = 9747;
			ectmask = 110;
			iat = 9741;
			iatmask = 98;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K2J-T01.bin";
			break;
		case "116. 38770-K03-T61":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = false;
			toolStripMenuItem13.Visible = false;
			toolStripMenuItem14.Visible = false;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 30282;
			rpmh = 30224;
			iaph = 0;
			feul1 = 31782;
			feul2 = 30332;
			feul3 = 0;
			iap = 0;
			ig1 = 33232;
			ig2 = 33957;
			ig3 = 0;
			ig4 = 0;
			kon = 3.6408696f;
			limit1 = 34752;
			limit2 = 3456;
			limit3 = 21032;
			limit4 = 8433;
			limit5 = 3456;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 39492;
			o2mask = 9;
			bass = 39498;
			bassmask = 14;
			ect = 39487;
			ectmask = 4;
			iat = 39489;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K03-T61.bin";
			break;
		case "117. 38770-K03-H01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 4548;
			limit2 = 44179;
			limit3 = 4858;
			limit4 = 4860;
			limit5 = 16048;
			limit6 = 38530;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 10373;
			o2mask = 9;
			bass = 10379;
			bassmask = 14;
			ect = 10368;
			ectmask = 4;
			iat = 10370;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K03-H01.bin";
			break;
		case "118. 38770-KWW-643":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = false;
			toolStripMenuItem13.Visible = false;
			toolStripMenuItem14.Visible = false;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 30282;
			rpmh = 30224;
			iaph = 0;
			feul1 = 31782;
			feul2 = 30332;
			feul3 = 0;
			iap = 0;
			ig1 = 33232;
			ig2 = 33957;
			ig3 = 0;
			ig4 = 0;
			kon = 3.6408696f;
			limit1 = 34752;
			limit2 = 3456;
			limit3 = 21032;
			limit4 = 8433;
			limit5 = 3456;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 39492;
			o2mask = 9;
			bass = 39498;
			bassmask = 14;
			ect = 39487;
			ectmask = 4;
			iat = 39489;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-KWW-643.bin";
			break;
		case "119. 38770-K58-N81":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 1814;
			feul2 = 132;
			feul3 = 0;
			iap = 0;
			ig1 = 3496;
			ig2 = 4337;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 44395;
			limit2 = 38628;
			limit3 = 45228;
			limit4 = 6467;
			limit5 = 15959;
			limit6 = 6093;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 11755;
			o2mask = 9;
			bass = 11761;
			bassmask = 14;
			ect = 11750;
			ectmask = 4;
			iat = 11752;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K58-N81.bin";
			break;
		case "120. 38770-K58R-N81":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 1814;
			feul2 = 132;
			feul3 = 0;
			iap = 0;
			ig1 = 3496;
			ig2 = 4337;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 44395;
			limit2 = 38628;
			limit3 = 45228;
			limit4 = 6467;
			limit5 = 15959;
			limit6 = 6093;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 11755;
			o2mask = 9;
			bass = 11761;
			bassmask = 14;
			ect = 11750;
			ectmask = 4;
			iat = 11752;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K58R-N81.bin";
			break;
		case "121. 38770-KYZ-712":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = false;
			toolStripMenuItem13.Visible = false;
			toolStripMenuItem14.Visible = false;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 30282;
			rpmh = 30224;
			iaph = 0;
			feul1 = 31782;
			feul2 = 30332;
			feul3 = 0;
			iap = 0;
			ig1 = 33232;
			ig2 = 33957;
			ig3 = 0;
			ig4 = 0;
			kon = 3.6408696f;
			limit1 = 3456;
			limit2 = 8215;
			limit3 = 0;
			limit4 = 0;
			limit5 = 0;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-KYZ-712.bin";
			break;
		case "122. 38770-K41K-NC1":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 1814;
			feul2 = 132;
			feul3 = 0;
			iap = 0;
			ig1 = 3496;
			ig2 = 4337;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 44395;
			limit2 = 38628;
			limit3 = 45228;
			limit4 = 6467;
			limit5 = 15959;
			limit6 = 6093;
			limit7 = 0;
			lm = 3.64086f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K41K-NC1.bin";
			break;
		case "123. 38770-K3F-T01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 1814;
			feul2 = 132;
			feul3 = 0;
			iap = 0;
			ig1 = 3496;
			ig2 = 4337;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 44395;
			limit2 = 38628;
			limit3 = 45228;
			limit4 = 6467;
			limit5 = 15959;
			limit6 = 6093;
			limit7 = 0;
			lm = 3.64086f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K3F-T01.bin";
			break;
		case "124. 38770-K73-T32":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 4548;
			limit2 = 16072;
			limit3 = 38646;
			limit4 = 44330;
			limit5 = 45163;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 10373;
			o2mask = 9;
			bass = 10379;
			bassmask = 14;
			ect = 10368;
			ectmask = 4;
			iat = 10370;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K73-T32.bin";
			break;
		case "125. 38770-K73-TC1":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 4548;
			limit2 = 16072;
			limit3 = 38646;
			limit4 = 44330;
			limit5 = 45163;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 10373;
			o2mask = 9;
			bass = 10379;
			bassmask = 14;
			ect = 10368;
			ectmask = 4;
			iat = 10370;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K73-TC1.bin";
			break;
		case "126. 38770-KYZ-T41":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 4548;
			limit2 = 44179;
			limit3 = 4858;
			limit4 = 4860;
			limit5 = 16048;
			limit6 = 38530;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 10373;
			o2mask = 9;
			bass = 10379;
			bassmask = 14;
			ect = 10368;
			ectmask = 4;
			iat = 10370;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-KYZ-T41.bin";
			break;
		case "127. 38770-KYZ-T03":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = false;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 30282;
			rpmh = 30224;
			iaph = 0;
			feul1 = 31782;
			feul2 = 30332;
			feul3 = 0;
			iap = 0;
			ig1 = 33232;
			ig2 = 33957;
			ig3 = 0;
			ig4 = 0;
			kon = 3.6408696f;
			limit1 = 34752;
			limit2 = 3456;
			limit3 = 8215;
			limit4 = 34752;
			limit5 = 3456;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 39492;
			o2mask = 9;
			bass = 39498;
			bassmask = 14;
			ect = 39487;
			ectmask = 4;
			iat = 39489;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-KYZ-T03.bin";
			break;
		case "128. 38770-K76-T62":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 5273;
			limit2 = 15959;
			limit3 = 45219;
			limit4 = 38628;
			limit5 = 44386;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 10935;
			o2mask = 9;
			bass = 10941;
			bassmask = 14;
			ect = 10930;
			ectmask = 4;
			iat = 10932;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K76-T62.bin";
			break;
		case "129. 38770-KVB-S52":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = false;
			toolStripMenuItem13.Visible = false;
			toolStripMenuItem14.Visible = false;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 30282;
			rpmh = 30224;
			iaph = 0;
			feul1 = 31782;
			feul2 = 30332;
			feul3 = 0;
			iap = 0;
			ig1 = 33232;
			ig2 = 33957;
			ig3 = 0;
			ig4 = 0;
			kon = 3.6408696f;
			limit1 = 34752;
			limit2 = 3456;
			limit3 = 21032;
			limit4 = 8433;
			limit5 = 3456;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 39492;
			o2mask = 9;
			bass = 39498;
			bassmask = 14;
			ect = 39487;
			ectmask = 4;
			iat = 39489;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-KVB-S52.bin";
			break;
		case "130. 38770-KZL-C01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 30282;
			rpmh = 30224;
			iaph = 0;
			feul1 = 31782;
			feul2 = 30332;
			feul3 = 0;
			iap = 0;
			ig1 = 33232;
			ig2 = 33957;
			ig3 = 0;
			ig4 = 0;
			kon = 3.6408696f;
			limit1 = 34752;
			limit2 = 9260;
			limit3 = 21032;
			limit4 = 8433;
			limit5 = 3456;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 39492;
			o2mask = 9;
			bass = 39498;
			bassmask = 14;
			ect = 39487;
			ectmask = 4;
			iat = 39489;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-KZL-C01.bin";
			break;
		case "131. 38770-K2E-T11":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 132;
			feul2 = 1814;
			feul3 = 0;
			iap = 0;
			ig1 = 3496;
			ig2 = 4337;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 6125;
			limit2 = 45374;
			limit3 = 15959;
			limit4 = 38734;
			limit5 = 44541;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 9736;
			o2mask = 90;
			bass = 9738;
			bassmask = 91;
			ect = 9747;
			ectmask = 110;
			iat = 9741;
			iatmask = 98;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K2E-T11.bin";
			break;
		case "132. 38770-K26-B02":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 4548;
			limit2 = 16072;
			limit3 = 38646;
			limit4 = 44330;
			limit5 = 45163;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 10373;
			o2mask = 9;
			bass = 10379;
			bassmask = 14;
			ect = 10368;
			ectmask = 4;
			iat = 10370;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K26-B02.bin";
			break;
		case "133. 38770-K26-921":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 30282;
			rpmh = 30224;
			iaph = 0;
			feul1 = 31782;
			feul2 = 30332;
			feul3 = 0;
			iap = 0;
			ig1 = 33232;
			ig2 = 33957;
			ig3 = 0;
			ig4 = 0;
			kon = 3.6408696f;
			limit1 = 34752;
			limit2 = 9260;
			limit3 = 21032;
			limit4 = 8433;
			limit5 = 3456;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 39492;
			o2mask = 9;
			bass = 39498;
			bassmask = 14;
			ect = 39487;
			ectmask = 4;
			iat = 39489;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K26-921.bin";
			break;
		case "134. 38770-K26-911":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 30282;
			rpmh = 30224;
			iaph = 0;
			feul1 = 31782;
			feul2 = 30332;
			feul3 = 0;
			iap = 0;
			ig1 = 33232;
			ig2 = 33957;
			ig3 = 0;
			ig4 = 0;
			kon = 3.6408696f;
			limit1 = 34752;
			limit2 = 9260;
			limit3 = 21032;
			limit4 = 8433;
			limit5 = 3456;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 39492;
			o2mask = 9;
			bass = 39498;
			bassmask = 14;
			ect = 39487;
			ectmask = 4;
			iat = 39489;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K26-911.bin";
			break;
		case "135. 38770-K26-901":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 30282;
			rpmh = 30224;
			iaph = 0;
			feul1 = 31782;
			feul2 = 30332;
			feul3 = 0;
			iap = 0;
			ig1 = 33232;
			ig2 = 33957;
			ig3 = 0;
			ig4 = 0;
			kon = 3.6408696f;
			limit1 = 34752;
			limit2 = 9260;
			limit3 = 21032;
			limit4 = 8433;
			limit5 = 3456;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 39492;
			o2mask = 9;
			bass = 39498;
			bassmask = 14;
			ect = 39487;
			ectmask = 4;
			iat = 39489;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K26-901.bin";
			break;
		case "136. 38770-K1M-T01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 132;
			feul2 = 1814;
			feul3 = 0;
			iap = 0;
			ig1 = 3496;
			ig2 = 4337;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 6125;
			limit2 = 45374;
			limit3 = 15959;
			limit4 = 38734;
			limit5 = 44541;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 9736;
			o2mask = 90;
			bass = 9738;
			bassmask = 91;
			ect = 9747;
			ectmask = 110;
			iat = 9741;
			iatmask = 98;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K1M-T01.bin";
			break;
		case "137. 38770-K1M-T31":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 132;
			feul2 = 1814;
			feul3 = 0;
			iap = 0;
			ig1 = 3496;
			ig2 = 4337;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 6125;
			limit2 = 45374;
			limit3 = 15959;
			limit4 = 38734;
			limit5 = 44541;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 9736;
			o2mask = 90;
			bass = 9738;
			bassmask = 91;
			ect = 9747;
			ectmask = 110;
			iat = 9741;
			iatmask = 98;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-K1M-T31.bin";
			break;
		case "138. 30400-K1AL-NA1":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = true;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 35;
			tpsh = 243822;
			rpmh = 243880;
			iaph = 0;
			feul1 = 236320;
			feul2 = 238350;
			feul3 = 248566;
			iap = 0;
			ig1 = 243985;
			ig2 = 245000;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235400;
			limit2 = 235634;
			limit3 = 236088;
			limit4 = 240592;
			limit5 = 240602;
			limit6 = 242394;
			limit7 = 242288;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 246741;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-BEAT NEW-K1AL-NA1...bin";
			break;
		case "139. 30400-K35A-T01":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 103238;
			rpmh = 103288;
			iaph = 103212;
			feul1 = 104796;
			feul2 = 257480;
			feul3 = 0;
			iap = 103346;
			ig1 = 106275;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 100094;
			limit2 = 102292;
			limit3 = 100692;
			limit4 = 102380;
			limit5 = 101058;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 109093;
			o2c = 266900;
			o2mask = 42;
			o2cmask = 33;
			IMMO = 254801;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K35A-T01.bin";
			break;
		case "140. 30400-K35A-V01":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 103238;
			rpmh = 103288;
			iaph = 103212;
			feul1 = 104796;
			feul2 = 257480;
			feul3 = 0;
			iap = 103346;
			ig1 = 106275;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 100094;
			limit2 = 102292;
			limit3 = 100692;
			limit4 = 102380;
			limit5 = 101058;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 109093;
			o2c = 266900;
			o2mask = 42;
			o2cmask = 33;
			IMMO = 254801;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K35A-V01.bin";
			break;
		case "141. 30400-K97A-U01":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = false;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 103988;
			rpmh = 104038;
			iaph = 103963;
			feul1 = 105858;
			feul2 = 0;
			feul3 = 0;
			iap = 104108;
			ig1 = 107643;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 100866;
			limit2 = 101236;
			limit3 = 102924;
			limit4 = 100618;
			limit5 = 101602;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 108864;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K97A-U01.bin";
			break;
		case "142. 38770-CAP-H01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 5273;
			limit2 = 15959;
			limit3 = 45237;
			limit4 = 38646;
			limit5 = 44404;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 10373;
			o2mask = 9;
			bass = 10379;
			bassmask = 14;
			ect = 10368;
			ectmask = 4;
			iat = 10370;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-CAP-H01.bin";
			break;
		case "143. 38770-KPG-H01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 5273;
			limit2 = 15959;
			limit3 = 45237;
			limit4 = 38646;
			limit5 = 44404;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 10373;
			o2mask = 9;
			bass = 10379;
			bassmask = 14;
			ect = 10368;
			ectmask = 4;
			iat = 10370;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-KPG-H01.bin";
			break;
		case "144. 38770-KPG-H21":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 5273;
			limit2 = 15959;
			limit3 = 45237;
			limit4 = 38646;
			limit5 = 44404;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 10373;
			o2mask = 9;
			bass = 10379;
			bassmask = 14;
			ect = 10368;
			ectmask = 4;
			iat = 10370;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-KPG-H21.bin";
			break;
		case "145. 38770-KPY-H01":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 5273;
			limit2 = 15959;
			limit3 = 45237;
			limit4 = 38646;
			limit5 = 44404;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 10373;
			o2mask = 9;
			bass = 10379;
			bassmask = 14;
			ect = 10368;
			ectmask = 4;
			iat = 10370;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-KPY-H01.bin";
			break;
		case "146. 38770-KPY-H61":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = true;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = true;
			offIATSensorToolStripMenuItem.Visible = true;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 74;
			rpmh = 16;
			iaph = 0;
			feul1 = 124;
			feul2 = 1574;
			feul3 = 0;
			iap = 0;
			ig1 = 3024;
			ig2 = 3749;
			ig3 = 0;
			ig4 = 0;
			kon = 3.64086f;
			limit1 = 5273;
			limit2 = 15959;
			limit3 = 45237;
			limit4 = 38646;
			limit5 = 44404;
			limit6 = 0;
			limit7 = 0;
			lm = 3.64086f;
			o2c = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			o2 = 10373;
			o2mask = 9;
			bass = 10379;
			bassmask = 14;
			ect = 10368;
			ectmask = 4;
			iat = 10370;
			iatmask = 6;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.38770-KPY-H61.bin";
			break;
		case "147. 38770-K46-N31":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 233644;
			rpmh = 233696;
			iaph = 0;
			feul1 = 233756;
			feul2 = 235208;
			feul3 = 0;
			iap = 0;
			ig1 = 238572;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 230932;
			limit2 = 231272;
			limit3 = 231424;
			limit4 = 231596;
			limit5 = 237002;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 224449;
			immomask = 64;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.VARIO110 FI LED 30400 K46-N31_BIN.bin";
			break;
		case "148. 30400-K1ZV-NA1":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 253096;
			rpmh = 253146;
			iaph = 253071;
			feul1 = 254966;
			feul2 = 262004;
			feul3 = 253216;
			iap = 265480;
			ig1 = 256751;
			ig2 = 257626;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 248504;
			limit2 = 248772;
			limit3 = 249590;
			limit4 = 249600;
			limit5 = 251116;
			limit6 = 251386;
			limit7 = 0;
			lm = 1f;
			o2 = 109093;
			o2c = 266900;
			o2mask = 42;
			o2cmask = 33;
			IMMO = 254801;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K1ZV-NA1.bin";
			break;
		case "149. 30400-K1ZV-NC1":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 253096;
			rpmh = 253146;
			iaph = 253071;
			feul1 = 254966;
			feul2 = 262004;
			feul3 = 253216;
			iap = 265480;
			ig1 = 256751;
			ig2 = 257626;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 248504;
			limit2 = 248772;
			limit3 = 249590;
			limit4 = 249600;
			limit5 = 251116;
			limit6 = 251386;
			limit7 = 0;
			lm = 1f;
			o2 = 109093;
			o2c = 266900;
			o2mask = 42;
			o2cmask = 33;
			IMMO = 254801;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-PCX 160-K1ZV-NC1.bin";
			break;
		case "150. 30400-K1ZV-NF1":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = true;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 253096;
			rpmh = 253146;
			iaph = 253071;
			feul1 = 254966;
			feul2 = 262004;
			feul3 = 253216;
			iap = 265480;
			ig1 = 256751;
			ig2 = 257626;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 248504;
			limit2 = 248772;
			limit3 = 249590;
			limit4 = 249600;
			limit5 = 251116;
			limit6 = 251386;
			limit7 = 0;
			lm = 1f;
			o2 = 109093;
			o2c = 266900;
			o2mask = 42;
			o2cmask = 33;
			IMMO = 254801;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-PCX 160-K1ZV NF1.bin";
			break;
		case "151. 30400-K2F-NC1":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 212390;
			rpmh = 212440;
			iaph = 0;
			feul1 = 212498;
			feul2 = 213948;
			feul3 = 0;
			iap = 0;
			ig1 = 219134;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 212014;
			limit2 = 215592;
			limit3 = 217412;
			limit4 = 217480;
			limit5 = 218110;
			limit6 = 217564;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 224449;
			immomask = 64;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.HONDA SCOOPY K2F-NC1.bin";
			break;
		case "152. 30400-K2F-ND1":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 212390;
			rpmh = 212440;
			iaph = 0;
			feul1 = 212498;
			feul2 = 213948;
			feul3 = 0;
			iap = 0;
			ig1 = 219134;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 212014;
			limit2 = 215592;
			limit3 = 217412;
			limit4 = 217480;
			limit5 = 218110;
			limit6 = 217564;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 224449;
			immomask = 64;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.SCUPY NEW KYLES 30400-K2F-ND1.bin";
			break;
		case "153. 30400-KOJ-NA1":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 212390;
			rpmh = 212440;
			iaph = 0;
			feul1 = 212498;
			feul2 = 213948;
			feul3 = 0;
			iap = 0;
			ig1 = 219134;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 212014;
			limit2 = 215592;
			limit3 = 217412;
			limit4 = 217480;
			limit5 = 218110;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-KOJ-NA1.bin";
			break;
		case "154. 30400-KOJ-NB1":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = false;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = false;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 29;
			tpsh = 212390;
			rpmh = 212440;
			iaph = 0;
			feul1 = 212498;
			feul2 = 213948;
			feul3 = 0;
			iap = 0;
			ig1 = 219134;
			ig2 = 0;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 212014;
			limit2 = 215592;
			limit3 = 217412;
			limit4 = 217480;
			limit5 = 218110;
			limit6 = 0;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 1;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-KOJ-NB1.bin";
			break;
		case "155. 30400-K2VM-N81":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 244276;
			rpmh = 244334;
			iaph = 0;
			feul1 = 236834;
			feul2 = 238864;
			feul3 = 0;
			ig1 = 244439;
			ig2 = 245454;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235916;
			limit2 = 236150;
			limit3 = 236604;
			limit4 = 241106;
			limit5 = 242908;
			limit6 = 241116;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K2VM-N81.bin";
			break;
		case "156. 30400-K2VM-NA1":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 244276;
			rpmh = 244334;
			iaph = 0;
			feul1 = 236834;
			feul2 = 238864;
			feul3 = 0;
			ig1 = 244439;
			ig2 = 245454;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235916;
			limit2 = 236150;
			limit3 = 236604;
			limit4 = 241106;
			limit5 = 242908;
			limit6 = 241116;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K2VM-NA1.bin";
			break;
		case "157. 30400-K2VM-V51":
			breakpointIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = false;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = false;
			toolStripMenuItem17.Visible = false;
			toolStripMenuItem18.Visible = false;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 29;
			rowCount = 29;
			tpsh = 244276;
			rpmh = 244334;
			iaph = 0;
			feul1 = 236834;
			feul2 = 238864;
			feul3 = 0;
			ig1 = 244439;
			ig2 = 245454;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 235916;
			limit2 = 236150;
			limit3 = 236604;
			limit4 = 241106;
			limit5 = 242908;
			limit6 = 241116;
			limit7 = 0;
			lm = 1f;
			o2 = 0;
			o2c = 0;
			o2mask = 0;
			o2cmask = 48;
			IMMO = 0;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-K2VM-V51.bin";
			break;
		case "158. 30400-KOWW-NC2":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 253300;
			rpmh = 253350;
			iaph = 253275;
			feul1 = 255170;
			feul2 = 262208;
			feul3 = 0;
			iap = 253420;
			ig1 = 256955;
			ig2 = 257830;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 251398;
			limit2 = 251450;
			limit3 = 251480;
			limit4 = 251510;
			limit5 = 252826;
			limit6 = 253416;
			limit7 = 0;
			lm = 1f;
			o2 = 271724;
			o2c = 259255;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 259294;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-KOWW-NC2.bin";
			break;
		case "159. 30400-KOWW-ND2":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 253300;
			rpmh = 253350;
			iaph = 253275;
			feul1 = 255170;
			feul2 = 262208;
			feul3 = 0;
			iap = 253420;
			ig1 = 256955;
			ig2 = 257830;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 251398;
			limit2 = 251450;
			limit3 = 251480;
			limit4 = 251510;
			limit5 = 252826;
			limit6 = 253416;
			limit7 = 0;
			lm = 1f;
			o2 = 271724;
			o2c = 259255;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 259294;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-KOWW-ND2.bin";
			break;
		case "160. 30400-KOWW-NE2":
			breakpointIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem7.Visible = true;
			toolStripMenuItem8.Visible = false;
			ignition4ToolStripMenuItem.Visible = false;
			ignition5ToolStripMenuItem.Visible = false;
			rpmVsTps2ToolStripMenuItem.Visible = true;
			rpmVsToolStripMenuItem.Visible = false;
			rpmVsIAPToolStripMenuItem.Visible = true;
			toolStripMenuItem12.Visible = true;
			toolStripMenuItem13.Visible = true;
			toolStripMenuItem14.Visible = true;
			limiter6ToolStripMenuItem.Visible = true;
			limiter7ToolStripMenuItem.Visible = false;
			toolStripMenuItem16.Visible = true;
			toolStripMenuItem17.Visible = true;
			toolStripMenuItem18.Visible = true;
			toolStripMenuItem19.Visible = false;
			toolStripMenuItem20.Visible = false;
			toolStripMenuItem21.Visible = false;
			toolStripMenuItem22.Visible = false;
			offMode4ToolStripMenuItem.Visible = false;
			offBASSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem.Visible = false;
			offECTSensorToolStripMenuItem.Visible = false;
			offIATSensorToolStripMenuItem.Visible = false;
			offVSSSensorToolStripMenuItem.Visible = false;
			offMAPSensorToolStripMenuItem1.Visible = false;
			columnCount = 25;
			rowCount = 35;
			tpsh = 253300;
			rpmh = 253350;
			iaph = 253275;
			feul1 = 255170;
			feul2 = 262208;
			feul3 = 0;
			iap = 253420;
			ig1 = 256955;
			ig2 = 257830;
			ig3 = 0;
			ig4 = 0;
			kon = 1f;
			limit1 = 251398;
			limit2 = 251450;
			limit3 = 251480;
			limit4 = 251510;
			limit5 = 252826;
			limit6 = 253416;
			limit7 = 0;
			lm = 1f;
			o2 = 271724;
			o2c = 259255;
			o2mask = 33;
			o2cmask = 1;
			IMMO = 259294;
			immomask = 1;
			MODE0 = 0;
			MODE1 = 0;
			MODE2 = 0;
			MODE3 = 0;
			MODE4 = 0;
			startAddress = feul1;
			startAddress2 = feul2;
			startAddress3 = ig1;
			text2 = "_3D_Chart_Graph.30400-KOWW-NE2.bin";
			break;
		}
		if (text2 != null)
		{
			panelDATA.Visible = true;
			LoadBinaryData(text2);
			UpdateBinIdentity();
		}
	}

	private void MoveTimer_Tick(object sender, EventArgs e)
	{
		MovePointer();
	}

	private void MovePointer()
	{
		oldPointerRow = pointerRow;
		oldPointerColumn = pointerColumn;
		pointerRow %= dataGridView1.RowCount;
		pointerColumn %= dataGridView1.ColumnCount;
		dataGridView1.InvalidateCell(oldPointerColumn, oldPointerRow);
		dataGridView1.InvalidateCell(pointerColumn, pointerRow);
	}

	private void DataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
	{
		if (e.RowIndex < 0 || e.ColumnIndex < 0)
		{
			return;
		}
		e.PaintBackground(e.CellBounds, cellsPaintSelectionBackground: true);
		e.PaintContent(e.CellBounds);
		Color color = Color.FromArgb(64, 64, 64);
		Color color2 = Color.FromArgb(192, 192, 192);
		using (Pen pen = new Pen(color))
		{
			using Pen pen2 = new Pen(color2);
			e.Graphics.DrawLine(pen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
			e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
			e.Graphics.DrawLine(pen2, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Top);
			e.Graphics.DrawLine(pen2, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Left, e.CellBounds.Bottom - 1);
		}
		if (showPointer && e.RowIndex == pointerRow && e.ColumnIndex == pointerColumn)
		{
			using Brush brush = new SolidBrush(Color.Black);
			int num = e.CellBounds.Left + (e.CellBounds.Width - PointerSize) / 2;
			int num2 = e.CellBounds.Top + (e.CellBounds.Height - PointerSize) / 2;
			e.Graphics.FillEllipse(brush, num, num2, PointerSize, PointerSize);
		}
		e.Handled = true;
	}

	private void CreateTable()
	{
		dataGridView1 = new DataGridView
		{
			ColumnCount = columnCount + 1,
			RowCount = rowCount,
			Dock = DockStyle.Fill,
			AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
			AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
			ScrollBars = ScrollBars.Both,
			ColumnHeadersVisible = false,
			RowHeadersVisible = false,
			BorderStyle = BorderStyle.Fixed3D,
			AllowUserToResizeColumns = false,
			AllowUserToResizeRows = false
		};
		float emSize = 13f;
		Font font = new Font("Arial", emSize, FontStyle.Regular);
		dataGridView1.DefaultCellStyle.Font = font;
		dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
		panelDATA.Controls.Add(dataGridView1);
		dataGridView1.CellFormatting += DataGridView1_CellFormatting;
		panelDATA.Resize += PanelDATA_Resize;
		AdjustColumnAndRowSizes();
	}

	private void PanelDATA_Resize(object sender, EventArgs e)
	{
		AdjustColumnAndRowSizes();
	}

	private void AdjustColumnAndRowSizes()
	{
		if (dataGridView1 == null || dataGridView1.ColumnCount <= 0 || dataGridView1.RowCount <= 0)
		{
			return;
		}
		int num = panelDATA.ClientSize.Width;
		int num2 = num / dataGridView1.ColumnCount;
		foreach (DataGridViewColumn column in dataGridView1.Columns)
		{
			column.Width = num2;
		}
		int num3 = panelDATA.ClientSize.Height;
		int num4 = num3 / dataGridView1.RowCount;
		foreach (DataGridViewRow item in (IEnumerable)dataGridView1.Rows)
		{
			item.Height = num4;
		}
	}

	private void ukuran()
	{
		if (dataGridView1 != null)
		{
			ukur = false;
			dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
			dataGridView1.Dock = DockStyle.Fill;
			AdjustColumnAndRowSizes();
			float emSize = 8f;
			Font font = new Font("Arial", emSize, FontStyle.Regular);
			dataGridView1.DefaultCellStyle.Font = font;
			dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dataGridView1.ColumnHeadersDefaultCellStyle.Font = font;
			dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dataGridView1.Refresh();
		}
	}

	private void ukuran2()
	{
		ukur = true;
		dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
		dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
		dataGridView1.Dock = DockStyle.None;
		int num = 50;
		dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
		foreach (DataGridViewColumn column in dataGridView1.Columns)
		{
			column.Width = num;
			column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
		}
		int num2 = 25;
		foreach (DataGridViewRow item in (IEnumerable)dataGridView1.Rows)
		{
			item.Height = num2;
		}
		dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
		dataGridView1.ColumnHeadersHeight = 35;
		float emSize = 8f;
		Font font = new Font("Arial", emSize, FontStyle.Regular);
		dataGridView1.DefaultCellStyle.Font = font;
		dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
		dataGridView1.ColumnHeadersDefaultCellStyle.Font = font;
		dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
		dataGridView1.Width = panelDATA.ClientSize.Width;
		dataGridView1.Height = panelDATA.ClientSize.Height;
		dataGridView1.Refresh();
	}

	private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
	{
		if (minValue == maxValue)
		{
			minValue = 0;
		}
		if (e.RowIndex == 0 || e.ColumnIndex == 0 || e.Value == null)
		{
			return;
		}
		string text = e.Value.ToString();
		float proportion = 0f;
		double result = 0.0;
		bool flag = false;
		if (!((!text.Contains('.') && !text.Contains(',')) ? double.TryParse(text, out result) : double.TryParse(text, out result)))
		{
			e.CellStyle.BackColor = Color.White;
			dataGridView1[e.ColumnIndex, e.RowIndex].ToolTipText = "-";
			return;
		}
		ushort result2;
		if (selectedStartAddress == ig1 || selectedStartAddress == ig2 || selectedStartAddress == ig3 || selectedStartAddress == ig4)
		{
			if (selectedStartAddress != 244144 && selectedStartAddress != 238572 && selectedStartAddress != 244144)
			{
				proportion = ((selectedStartAddress != 210634 && selectedStartAddress != 219134) ? ((float)(((result + 14.7) / 87.85 * 255.0 - (double)(int)minValue) / (double)(maxValue - minValue))) : ((float)((result * 17.0 - (double)(int)minValue) / (double)(maxValue - minValue))));
			}
			else
			{
				proportion = (float)(((result + 11.53) / 19.827 * 255.0 - (double)(int)minValue) / (double)(maxValue - minValue));
			}
		}
		else if (selectedStartAddress == tpsh || selectedStartAddress == iaph)
		{
			proportion = (float)((result * 100.0 - (double)(int)minValue) / (double)(maxValue - minValue));
		}
		else if (selectedStartAddress == feul1 || selectedStartAddress == feul2 || selectedStartAddress == feul3 || selectedStartAddress == iap)
		{
			proportion = (float)((result * 1000.0 - (double)(int)minValue) / (double)(maxValue - minValue));
		}
		else if (selectedStartAddress == rpmh || selectedStartAddress == limit1 || selectedStartAddress == limit2 || selectedStartAddress == limit3 || selectedStartAddress == limit4 || selectedStartAddress == limit5 || selectedStartAddress == limit6 || selectedStartAddress == limit7)
		{
			proportion = (float)((result - (double)(int)minValue) / (double)(maxValue - minValue));
		}
		else if (ushort.TryParse(text, out result2))
		{
			proportion = (result2 - minValue) / (maxValue - minValue);
		}
		e.CellStyle.BackColor = ColorFromProportion(proportion);
		dataGridView1[e.ColumnIndex, e.RowIndex].ToolTipText = $"Value: {result:F2}";
	}

	private Color ColorFromProportion(float proportion)
	{
		proportion = Math.Max(0f, Math.Min(1f, proportion));
		Color[] array = new Color[5]
		{
			Color.Blue,
			Color.Cyan,
			Color.FromArgb(0, 255, 0),
			Color.Yellow,
			Color.Red
		};
		float num = 1f / (float)(array.Length - 1);
		int num2 = (int)(proportion / num);
		int num3 = Math.Min(num2 + 1, array.Length - 1);
		float num4 = (proportion - (float)num2 * num) / num;
		Color color = array[num2];
		Color color2 = array[num3];
		int red = (int)((float)(int)color.R * (1f - num4) + (float)(int)color2.R * num4);
		int green = (int)((float)(int)color.G * (1f - num4) + (float)(int)color2.G * num4);
		int blue = (int)((float)(int)color.B * (1f - num4) + (float)(int)color2.B * num4);
		return Color.FromArgb(red, green, blue);
	}

	private void PrintResourceNames()
	{
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
		string[] array = manifestResourceNames;
		foreach (string value in array)
		{
			Console.WriteLine(value);
		}
	}

	private void LoadBinaryData(string resourceName)
	{
		SaveState();
		byte[] loaded = ReadBinSource(resourceName);
		if (loaded == null)
		{
			MessageBox.Show("Resource not found.");
			return;
		}
		binBytes = loaded;
		binBytesORI = (byte[])loaded.Clone();
		binSize = binBytes.Length;
		undoStack.Clear();
		redoStack.Clear();
		undoStack.Push((byte[])binBytes.Clone());
		DisplayData();
		dataGridView1.Visible = true;
		BtnWrite.Enabled = true;
		if (isUnknown)
		{
			GetOffsetsFromBinType();
		}
	}

	private void PrintTPSAndRPMValues()
	{
		Debug.WriteLine("TPS Values (X-Axis):");
		foreach (double tpsValue in tpsValues)
		{
			Debug.WriteLine(tpsValue);
		}
		Debug.WriteLine("RPM Values (Y-Axis):");
		foreach (double rpmValue in rpmValues)
		{
			Debug.WriteLine(rpmValue);
		}
	}

	private int FindClosestIndex(List<double> values, double target)
	{
		int result = 0;
		double num = Math.Abs(values[0] - target);
		for (int i = 1; i < values.Count; i++)
		{
			double num2 = Math.Abs(values[i] - target);
			if (num2 < num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	private void FindAndPrintClosestValues()
	{
		Debug.WriteLine($"Original RPMD: {RPMD}, TPSD: {TPSD}");
		int num = FindClosestIndex(rpmValues, RPMD);
		Debug.WriteLine($"Closest RPM value: {rpmValues[num]} at index {num}");
		if (selectedStartAddress == iap)
		{
			int num2 = FindClosestIndex(mapValues, MAP);
			Debug.WriteLine($"Closest map value: {mapValues[num2]} at index {num2}");
			Debug.WriteLine($"Pointer is at RPM row: {num + 1}, map column: {num2 + 1}");
			pointerRow = num + 1;
			pointerColumn = num2 + 1;
		}
		else
		{
			int num3 = FindClosestIndex(tpsValues, TPSD);
			Debug.WriteLine($"Closest TPS value: {tpsValues[num3]} at index {num3}");
			Debug.WriteLine($"Pointer is at RPM row: {num + 1}, TPS column: {num3 + 1}");
			pointerRow = num + 1;
			pointerColumn = num3 + 1;
		}
		UpdatePointerPosition();
	}

	private void DisplayData()
	{
		dataGridView1.Rows.Clear();
		dataGridView1.Columns.Clear();
		selectedStartAddress = feul1;
		int num = rowCount + 1;
		int num2 = columnCount;
		for (int i = 0; i < num2 + 1; i++)
		{
			dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
		}
		for (int j = 0; j < num; j++)
		{
			dataGridView1.Rows.Add();
		}
		tpsValues.Clear();
		rpmValues.Clear();
		if (feul1 == 239620 || feul1 == 233756 || customBigEndian)
		{
			for (int k = 1; k < num2 + 1; k++)
			{
				int num3 = tpsh + (k - 1) * 2;
				if (num3 + 1 < binBytes.Length)
				{
					ushort num4 = BitConverter.ToUInt16(new byte[2]
					{
						binBytes[num3 + 1],
						binBytes[num3]
					}, 0);
					double item = (double)(int)num4 / 10.0;
					dataGridView1[k, 0].Value = item.ToString("F1");
					dataGridView1[k, 0].Style.BackColor = Color.LightGray;
					dataGridView1[k, 0].Style.ForeColor = Color.Black;
					dataGridView1[k, 0].ReadOnly = true;
					tpsValues.Add(item);
				}
				else
				{
					dataGridView1[k, 0].Value = "N/A";
					dataGridView1[k, 0].Style.BackColor = Color.LightGray;
					dataGridView1[k, 0].Style.ForeColor = Color.Black;
					dataGridView1[k, 0].ReadOnly = true;
				}
			}
			for (int l = 1; l < num; l++)
			{
				int num5 = rpmh + (l - 1) * 2;
				if (num5 + 1 < binBytes.Length)
				{
					ushort num6 = BitConverter.ToUInt16(new byte[2]
					{
						binBytes[num5 + 1],
						binBytes[num5]
					}, 0);
					double item2 = (float)(int)num6 / kon;
					dataGridView1[0, l].Value = item2.ToString("F0");
					dataGridView1[0, l].Style.BackColor = Color.LightGray;
					dataGridView1[0, l].Style.ForeColor = Color.Black;
					dataGridView1[0, l].ReadOnly = true;
					rpmValues.Add(item2);
				}
				else
				{
					dataGridView1[0, l].Value = "N/A";
					dataGridView1[0, l].Style.BackColor = Color.LightGray;
					dataGridView1[0, l].Style.ForeColor = Color.Black;
					dataGridView1[0, l].ReadOnly = true;
				}
			}
			minValue = ushort.MaxValue;
			maxValue = 0;
			for (int m = 1; m < num; m++)
			{
				for (int n = 1; n < num2 + 1; n++)
				{
					int num7 = feul1 + ((m - 1) * num2 + (n - 1)) * 2;
					if (num7 + 1 < binBytes.Length)
					{
						ushort num8 = BitConverter.ToUInt16(new byte[2]
						{
							binBytes[num7 + 1],
							binBytes[num7]
						}, 0);
						if (num8 < minValue)
						{
							minValue = num8;
						}
						if (num8 > maxValue)
						{
							maxValue = num8;
						}
						double num9 = (double)(int)num8 / 1000.0;
						dataGridView1[n, m].Value = num9.ToString("F3");
					}
					else
					{
						dataGridView1[n, m].Value = "N/A";
					}
				}
			}
		}
		else
		{
			for (int num10 = 1; num10 < num2 + 1; num10++)
			{
				int num11 = tpsh + (num10 - 1) * 2;
				if (num11 + 1 < binBytes.Length)
				{
					ushort num12 = BitConverter.ToUInt16(new byte[2]
					{
						binBytes[num11],
						binBytes[num11 + 1]
					}, 0);
					double item3 = (double)(int)num12 / 10.0;
					dataGridView1[num10, 0].Value = item3.ToString("F1");
					dataGridView1[num10, 0].Style.BackColor = Color.LightGray;
					dataGridView1[num10, 0].Style.ForeColor = Color.Black;
					dataGridView1[num10, 0].ReadOnly = true;
					tpsValues.Add(item3);
				}
				else
				{
					dataGridView1[num10, 0].Value = "N/A";
					dataGridView1[num10, 0].Style.BackColor = Color.LightGray;
					dataGridView1[num10, 0].Style.ForeColor = Color.Black;
					dataGridView1[num10, 0].ReadOnly = true;
				}
			}
			for (int num13 = 1; num13 < num; num13++)
			{
				int num14 = rpmh + (num13 - 1) * 2;
				if (num14 + 1 < binBytes.Length)
				{
					ushort num15 = BitConverter.ToUInt16(new byte[2]
					{
						binBytes[num14],
						binBytes[num14 + 1]
					}, 0);
					double item4 = (float)(int)num15 / kon;
					dataGridView1[0, num13].Value = item4.ToString("F0");
					dataGridView1[0, num13].Style.BackColor = Color.LightGray;
					dataGridView1[0, num13].Style.ForeColor = Color.Black;
					dataGridView1[0, num13].ReadOnly = true;
					rpmValues.Add(item4);
				}
				else
				{
					dataGridView1[0, num13].Value = "N/A";
					dataGridView1[0, num13].Style.BackColor = Color.LightGray;
					dataGridView1[0, num13].Style.ForeColor = Color.Black;
					dataGridView1[0, num13].ReadOnly = true;
				}
			}
			minValue = ushort.MaxValue;
			maxValue = 0;
			for (int num16 = 1; num16 < num; num16++)
			{
				for (int num17 = 1; num17 < num2 + 1; num17++)
				{
					int num18 = feul1 + ((num16 - 1) * num2 + (num17 - 1)) * 2;
					if (num18 + 1 < binBytes.Length)
					{
						ushort num19 = BitConverter.ToUInt16(new byte[2]
						{
							binBytes[num18],
							binBytes[num18 + 1]
						}, 0);
						if (num19 < minValue)
						{
							minValue = num19;
						}
						if (num19 > maxValue)
						{
							maxValue = num19;
						}
						double num20 = (double)(int)num19 / 1000.0;
						dataGridView1[num17, num16].Value = num20.ToString("F3");
					}
					else
					{
						dataGridView1[num17, num16].Value = "N/A";
					}
				}
			}
		}
		ukuran();
		PrintTPSAndRPMValues();
	}

	// ---- Pengaman salah pilih bin: cocokkan file dengan tipe ECU yang dipilih ----
	private bool AxesLookValid(byte[] data)
	{
		bool bigEndian = feul1 == 239620 || feul1 == 233756 || customBigEndian;
		for (int pass = 0; pass < 2; pass++)
		{
			int start = (pass == 0) ? tpsh : rpmh;
			int count = (pass == 0) ? columnCount : rowCount;
			if (start <= 0 || count < 2 || start + count * 2 > data.Length)
			{
				return false;
			}
			int first = -1;
			int prev = -1;
			for (int i = 0; i < count; i++)
			{
				int p = start + i * 2;
				int v = bigEndian ? ((data[p] << 8) | data[p + 1]) : (data[p] | (data[p + 1] << 8));
				if (v < prev)
				{
					return false;
				}
				if (i == 0)
				{
					first = v;
				}
				prev = v;
			}
			if (prev <= first)
			{
				return false;
			}
		}
		return true;
	}

	private bool ConfirmBinMatchesSelection(byte[] candidate, string action)
	{
		try
		{
			if (candidate == null || binBytesORI == null || isUnknown || tpsh <= 0 || rpmh <= 0 || comboBox1.SelectedIndex < 0)
			{
				return true;
			}
			List<string> problems = new List<string>();
			if (candidate.Length != binBytesORI.Length)
			{
				problems.Add("Ukuran file " + candidate.Length.ToString("N0") + " byte, seharusnya " + binBytesORI.Length.ToString("N0") + " byte.");
			}
			else
			{
				int same = 0;
				for (int i = 0; i < candidate.Length; i++)
				{
					if (candidate[i] == binBytesORI[i])
					{
						same++;
					}
				}
				double similarity = (double)same * 100.0 / (double)candidate.Length;
				if (similarity < 75.0)
				{
					problems.Add("Isi file hanya " + similarity.ToString("F0") + "% mirip dengan bin tipe ini.");
				}
			}
			if (!AxesLookValid(candidate))
			{
				problems.Add("Sumbu TPS/RPM di alamat tipe ini tidak terbaca wajar.");
			}
			if (problems.Count == 0)
			{
				return true;
			}
			string message = "File bin kemungkinan TIDAK COCOK dengan tipe ECU yang dipilih:\r\n" + comboBox1.Text + "\r\n\r\n- " + string.Join("\r\n- ", problems) + "\r\n\r\nPastikan Anda memilih tipe dan file yang benar. Tetap lanjutkan " + action + "?";
			return MessageBox.Show(message, "Peringatan: Salah Pilih File?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
		}
		catch (Exception)
		{
			return true;
		}
	}

	private static ushort ClampU16(double value)
	{
		double rounded = Math.Round(value, MidpointRounding.AwayFromZero);
		if (rounded < 0.0)
		{
			return 0;
		}
		if (rounded > 65535.0)
		{
			return ushort.MaxValue;
		}
		return (ushort)rounded;
	}

	private void AdjustSelectedCells(int sign, bool percent, bool big)
	{
		if (isCompared)
		{
			RunWithRealValues(() => AdjustSelectedCells(sign, percent, big));
			return;
		}
		if (binBytes == null || dataGridView1 == null || dataGridView1.SelectedCells.Count == 0)
		{
			return;
		}
		int decimals = GetGridDecimals();
		double step;
		if (!double.TryParse(txtInput.Text, out step) || step <= 0.0)
		{
			step = Math.Pow(10.0, -decimals) * ((decimals >= 3) ? 10.0 : 1.0);
		}
		if (big)
		{
			step *= 10.0;
		}
		double cap = 65535.0 / Math.Pow(10.0, decimals);
		int changed = 0;
		foreach (DataGridViewCell cell in dataGridView1.SelectedCells)
		{
			if (cell.RowIndex < 1 || cell.ColumnIndex < 1 || cell.Value == null)
			{
				continue;
			}
			double value;
			if (!double.TryParse(cell.Value.ToString(), out value))
			{
				continue;
			}
			double next = percent ? (value * (1.0 + (double)sign * 0.01)) : (value + (double)sign * step);
			if (next < 0.0)
			{
				next = 0.0;
			}
			if (next > cap)
			{
				next = cap;
			}
			next = Math.Round(next, decimals);
			if (next != value)
			{
				cell.Value = next;
				changed++;
			}
		}
		if (changed == 0)
		{
			return;
		}
		button9.Enabled = true;
		SaveEditedValues2();
		SaveState();
		dataGridView1.Invalidate();
		RefreshOpenGraph();
	}

	private void RefreshOpenGraph()
	{
		try
		{
			foreach (Graph3DMainForm item in Application.OpenForms.OfType<Graph3DMainForm>().ToList())
			{
				item.UpdateGraphWithData(null);
			}
			SyncGraphSelection();
		}
		catch (Exception)
		{
		}
	}

	private void SyncGraphSelection()
	{
		try
		{
			DataGridViewCell currentCell = dataGridView1.CurrentCell;
			if (currentCell == null || currentCell.RowIndex < 1 || currentCell.ColumnIndex < 1)
			{
				return;
			}
			foreach (Graph3DMainForm item in Application.OpenForms.OfType<Graph3DMainForm>().ToList())
			{
				item.SelectGridCell(currentCell.RowIndex, currentCell.ColumnIndex, dataGridView1.RowCount);
			}
		}
		catch (Exception)
		{
		}
	}

	private void DataGridView1_SelectionChangedSync(object sender, EventArgs e)
	{
		SyncGraphSelection();
	}

	public void CommitGraphEdit()
	{
		if (binBytes != null)
		{
			SaveEditedValues2();
			SaveState();
		}
	}

	public static string BinPartCode = "";

	public static string BinEcuType = "";

	private string lastSavePath = "";

	private string baseTitle = "";

	private void UpdateBinIdentity()
	{
		try
		{
			string text = "";
			if (TxtPartCode != null)
			{
				System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(TxtPartCode.Text ?? "", "[0-9]{5}-[0-9A-Z]{2,5}(-[0-9A-Z]{3})?");
				if (match.Success)
				{
					text = match.Value;
				}
			}
			if (text.Length == 0 && comboBox1.SelectedItem != null)
			{
				System.Text.RegularExpressions.Match match2 = System.Text.RegularExpressions.Regex.Match(comboBox1.SelectedItem.ToString(), "[0-9]{5}-[0-9A-Z]{2,5}(-[0-9A-Z]{3})?");
				if (match2.Success)
				{
					text = match2.Value;
				}
			}
			BinPartCode = System.Text.RegularExpressions.Regex.Replace(text, "(?<=-)KO", "K0");
			BinEcuType = (binBytes != null) ? GetEcuType().ToString() : "";
		}
		catch (Exception)
		{
		}
	}

	private int compareMode = 1;

	private string FormatCompareText(double current, double baseValue, string deltaFormat)
	{
		if (compareMode == 2)
		{
			double percent = ((Math.Abs(baseValue) > 1E-09) ? ((current - baseValue) / Math.Abs(baseValue) * 100.0) : ((Math.Abs(current) > 1E-09) ? 100.0 : 0.0));
			return "(" + percent.ToString("+0.0;-0.0;0.0", CultureInfo.InvariantCulture) + "%)";
		}
		return "(" + (current - baseValue).ToString(deltaFormat, CultureInfo.InvariantCulture) + ")";
	}

	private void RefreshCurrentTable()
	{
		if (binBytes == null || binBytesORI == null)
		{
			return;
		}
		int a = selectedStartAddress;
		if (a == 0)
		{
			return;
		}
		if (a == feul1 || a == feul3)
		{
			TPS2Values();
		}
		else if (a == feul2)
		{
			TPS3Values();
		}
		else if (a == ig1 || a == ig2 || a == ig3 || a == ig4)
		{
			IGNITION1();
		}
		else if (a == iap)
		{
			IAPValues();
		}
		else if (a == limit1 || a == limit2 || a == limit3 || a == limit4 || a == limit5 || a == limit6 || a == limit7)
		{
			limiter1();
		}
		else if (a == rpmh)
		{
			RPMBREAKPOIN();
		}
		else if (a == tpsh)
		{
			TPSBREAK();
		}
		else if (a == iaph)
		{
			IAPBREAK();
		}
	}

	private void RestoreSelection(List<Point> cells)
	{
		try
		{
			dataGridView1.ClearSelection();
			foreach (Point cell in cells)
			{
				if (cell.Y >= 0 && cell.Y < dataGridView1.RowCount && cell.X >= 0 && cell.X < dataGridView1.ColumnCount)
				{
					dataGridView1[cell.X, cell.Y].Selected = true;
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private void RunWithRealValues(Action action)
	{
		if (!isCompared)
		{
			action();
			return;
		}
		List<Point> list = new List<Point>();
		foreach (DataGridViewCell selectedCell in dataGridView1.SelectedCells)
		{
			list.Add(new Point(selectedCell.ColumnIndex, selectedCell.RowIndex));
		}
		isCompared = false;
		RefreshCurrentTable();
		RestoreSelection(list);
		try
		{
			action();
		}
		finally
		{
			isCompared = true;
			RefreshCurrentTable();
			RestoreSelection(list);
		}
	}

	private double[,] CaptureGridFromBytes(byte[] bytes)
	{
		byte[] backup = binBytes;
		bool wasCompared = isCompared;
		try
		{
			isCompared = false;
			binBytes = bytes;
			RefreshCurrentTable();
			return GetGridData();
		}
		finally
		{
			binBytes = backup;
			isCompared = wasCompared;
			RefreshCurrentTable();
		}
	}

	private void CopyFromCompare()
	{
		if (binBytes == null || binBytesORI == null)
		{
			return;
		}
		List<Point> list = new List<Point>();
		foreach (DataGridViewCell selectedCell in dataGridView1.SelectedCells)
		{
			if (selectedCell.RowIndex >= 1 && selectedCell.ColumnIndex >= 1)
			{
				list.Add(new Point(selectedCell.ColumnIndex, selectedCell.RowIndex));
			}
		}
		if (list.Count == 0)
		{
			MessageBox.Show("Pilih dulu sel di tabel yang ingin diisi dari data compare.", "Copy from Compare", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
		RunWithRealValues(() =>
		{
			double[,] source = CaptureGridFromBytes((byte[])binBytesORI.Clone());
			int decimals = GetGridDecimals();
			foreach (Point cell in list)
			{
				if (cell.Y < source.GetLength(0) && cell.X < source.GetLength(1))
				{
					dataGridView1[cell.X, cell.Y].Value = Math.Round(source[cell.Y, cell.X], decimals);
				}
			}
			SaveEditedValues2();
			SaveState();
			RestoreSelection(list);
		});
		button9.Enabled = true;
		RefreshOpenGraph();
	}

	private void CompareToggle_Click(object sender, EventArgs e)
	{
		if (binBytes == null)
		{
			return;
		}
		isCompared = !isCompared;
		RefreshCurrentTable();
		UpdateCompareMenuChecks();
	}

	private void CompareModeValue_Click(object sender, EventArgs e)
	{
		compareMode = 1;
		if (isCompared)
		{
			RefreshCurrentTable();
		}
		UpdateCompareMenuChecks();
	}

	private void CompareModePercent_Click(object sender, EventArgs e)
	{
		compareMode = 2;
		if (isCompared)
		{
			RefreshCurrentTable();
		}
		UpdateCompareMenuChecks();
	}

	private void CompareLoadBin_Click(object sender, EventArgs e)
	{
		if (binBytes == null)
		{
			return;
		}
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Bin pembanding (*.bin;*.acg)|*.bin;*.acg|Semua file (*.*)|*.*";
		openFileDialog.Title = "Pilih bin pembanding";
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		try
		{
			byte[] array = File.ReadAllBytes(openFileDialog.FileName);
			if (openFileDialog.FileName.EndsWith(".acg", StringComparison.OrdinalIgnoreCase))
			{
				array = DecryptBytes2(array);
			}
			if (array.Length != binBytes.Length)
			{
				MessageBox.Show("Ukuran file pembanding berbeda (" + array.Length.ToString("N0") + " byte, bin aktif " + binBytes.Length.ToString("N0") + " byte). Pilih bin dengan tipe ECU yang sama.", "Compare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			binBytesORI = array;
			isCompared = true;
			RefreshCurrentTable();
			UpdateCompareMenuChecks();
		}
		catch (Exception ex)
		{
			MessageBox.Show("Gagal membaca bin pembanding: " + ex.Message, "Compare", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void CompareSetBase_Click(object sender, EventArgs e)
	{
		if (binBytes == null)
		{
			return;
		}
		RunWithRealValues(() => SaveEditedValues2());
		binBytesORI = (byte[])binBytes.Clone();
		RefreshCurrentTable();
	}

	private ToolStripMenuItem menuCompareToggle;

	private ToolStripMenuItem menuCompareValue;

	private ToolStripMenuItem menuComparePercent;

	private void UpdateCompareMenuChecks()
	{
		if (menuCompareToggle != null)
		{
			menuCompareToggle.Checked = isCompared;
			menuCompareValue.Checked = compareMode == 1;
			menuComparePercent.Checked = compareMode == 2;
		}
	}

	private void BuildExtraMenus()
	{
		Font font = fileToolStripMenuItem.Font;
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem("\ud83d\udcbe Save");
		toolStripMenuItem.Font = font;
		toolStripMenuItem.ToolTipText = "Simpan editan (Ctrl+S)";
		toolStripMenuItem.Click += delegate
		{
			QuickSave();
		};
		ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem("\u21a9 Undo");
		toolStripMenuItem2.Font = font;
		toolStripMenuItem2.ToolTipText = "Batalkan (Ctrl+Z)";
		toolStripMenuItem2.Click += delegate
		{
			Undo();
		};
		ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem("\u21aa Redo");
		toolStripMenuItem3.Font = font;
		toolStripMenuItem3.ToolTipText = "Ulangi (Ctrl+Y)";
		toolStripMenuItem3.Click += delegate
		{
			Redo();
		};
		ToolStripMenuItem toolStripMenuItem4 = new ToolStripMenuItem("\ud83d\udd0d Compare");
		toolStripMenuItem4.Font = font;
		menuCompareToggle = new ToolStripMenuItem("Tampilkan Compare (selisih dari bin dasar)");
		menuCompareToggle.Click += CompareToggle_Click;
		menuCompareValue = new ToolStripMenuItem("Mode: selisih nilai (+/-)");
		menuCompareValue.Click += CompareModeValue_Click;
		menuComparePercent = new ToolStripMenuItem("Mode: selisih persen (%)");
		menuComparePercent.Click += CompareModePercent_Click;
		ToolStripMenuItem toolStripMenuItem5 = new ToolStripMenuItem("Muat bin pembanding...");
		toolStripMenuItem5.Click += CompareLoadBin_Click;
		ToolStripMenuItem toolStripMenuItem6 = new ToolStripMenuItem("Jadikan bin saat ini sebagai dasar");
		toolStripMenuItem6.Click += CompareSetBase_Click;
		toolStripMenuItem4.DropDownItems.AddRange(new ToolStripItem[6] { menuCompareToggle, menuCompareValue, menuComparePercent, new ToolStripSeparator(), toolStripMenuItem5, toolStripMenuItem6 });
		int index = menuStrip7.Items.IndexOf(fileToolStripMenuItem) + 1;
		menuStrip7.Items.Insert(index, toolStripMenuItem);
		menuStrip7.Items.Insert(index + 1, toolStripMenuItem2);
		menuStrip7.Items.Insert(index + 2, toolStripMenuItem3);
		menuStrip7.Items.Insert(index + 3, toolStripMenuItem4);
		UpdateCompareMenuChecks();
	}

	private GroupBox groupBoxAfr;

	private Label lblAfrHeader;

	private void BuildAfrBox()
	{
		groupBoxAfr = new GroupBox();
		groupBoxAfr.Text = "AFR";
		groupBoxAfr.ForeColor = Color.Cyan;
		groupBoxAfr.Font = groupBox6.Font;
		groupBoxAfr.Anchor = groupBox6.Anchor;
		groupBoxAfr.TabStop = false;
		lblAfrHeader = new Label();
		lblAfrHeader.BackColor = Color.Black;
		lblAfrHeader.BorderStyle = BorderStyle.Fixed3D;
		lblAfrHeader.FlatStyle = FlatStyle.Popup;
		lblAfrHeader.Font = label11.Font;
		lblAfrHeader.ForeColor = label11.ForeColor;
		lblAfrHeader.TextAlign = ContentAlignment.MiddleCenter;
		lblAfrHeader.Text = "0.0";
		lblAfrHeader.Bounds = label11.Bounds;
		groupBoxAfr.Controls.Add(lblAfrHeader);
		groupBoxAfr.Size = groupBox6.Size;
		groupBoxAfr.Top = groupBox6.Top;
		Controls.Add(groupBoxAfr);
		groupBoxAfr.BringToFront();
	}

	private void LayoutHeaderBoxes()
	{
		if (groupBoxAfr == null)
		{
			return;
		}
		GroupBox[] array = new GroupBox[7] { groupBox1, groupBox2, groupBox3, groupBox4, groupBox5, groupBox6, groupBoxAfr };
		Label[] array2 = new Label[7] { label7, label6, label8, label5, label4, label11, lblAfrHeader };
		int left = Math.Min(groupBox1.Left, 313);
		int num = ClientSize.Width - 8;
		int num2 = 6;
		int num3 = (num - left - num2 * (array.Length - 1)) / array.Length;
		if (num3 < 60)
		{
			return;
		}
		float emSize = Math.Max(14f, Math.Min(39.75f, (float)(num3 - 12) / 3.6f));
		int top = groupBox1.Top;
		int height = groupBox1.Height;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Anchor = AnchorStyles.Top | AnchorStyles.Left;
			array[i].SetBounds(left + i * (num3 + num2), top, num3, height);
			array2[i].SetBounds(6, 18, num3 - 12, height - 24);
			if (Math.Abs(array2[i].Font.Size - emSize) > 0.1f)
			{
				array2[i].Font = new Font(array2[i].Font.FontFamily, emSize, array2[i].Font.Style);
			}
		}
	}

	private void UpdateAfrDisplay()
	{
		if (lblAfrHeader == null || lblAfrHeader.IsDisposed)
		{
			return;
		}
		Action action = delegate
		{
			lblAfrHeader.Text = AFR.ToString("F1");
			float num = AFR;
			Color foreColor = Color.Lime;
			if (num <= 0.5f)
			{
				foreColor = Color.Gray;
			}
			else if (num < 11.5f || num > 16.5f)
			{
				foreColor = Color.Red;
			}
			else if (num < 12.5f || num > 15.5f)
			{
				foreColor = Color.Orange;
			}
			lblAfrHeader.ForeColor = foreColor;
		};
		if (lblAfrHeader.InvokeRequired)
		{
			lblAfrHeader.BeginInvoke(action);
		}
		else
		{
			action();
		}
	}

	private void TryAutoCc()
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		string s = (textBox1.Text ?? "").Replace(',', '.');
		string s2 = (textBox2.Text ?? "").Replace(',', '.');
		if (float.TryParse(s, NumberStyles.Float, invariantCulture, out var result) && float.TryParse(s2, NumberStyles.Float, invariantCulture, out var result2) && result > 0f && result2 > 0f)
		{
			cc = 3.14f * (result / 2f * (result / 2f)) * result2 / 1000f;
			SetControlText(label13, cc.ToString("F3") + "  cm3");
		}
	}

	private void ExtraStartup()
	{
		baseTitle = Text;
		BuildExtraMenus();
		BuildAfrBox();
		try
		{
			InitAppearance();
		}
		catch (Exception)
		{
		}
		LayoutHeaderBoxes();
		base.Resize += delegate
		{
			LayoutHeaderBoxes();
		};
		button9.ShortcutKeys = Keys.Control | Keys.S;
		button9.Enabled = true;
		button9.ToolTipText = "Simpan editan (Ctrl+S)";
		try
		{
			LoadCustomProfiles();
		}
		catch (Exception)
		{
		}
		ApplyH2Theme();
	}

	public void SetCellValueFromGraph(int row, int col, double value)
	{
		if (isCompared)
		{
			return;
		}
		if (row < 1 || row >= dataGridView1.RowCount || col < 1 || col >= dataGridView1.ColumnCount)
		{
			return;
		}
		dataGridView1[col, row].Value = Math.Round(value, GetGridDecimals());
		if (binBytes != null)
		{
			button9.Enabled = true;
			SaveEditedValues2();
		}
	}

	public int GetGridDecimals()
	{
		int best = -1;
		for (int i = 1; i < dataGridView1.RowCount; i++)
		{
			for (int j = 1; j < dataGridView1.ColumnCount; j++)
			{
				object value = dataGridView1[j, i].Value;
				if (value == null)
				{
					continue;
				}
				string text = value.ToString();
				int sep = text.LastIndexOfAny(new char[] { '.', ',' });
				int places = (sep < 0) ? 0 : Math.Min(4, text.Length - sep - 1);
				if (places > best)
				{
					best = places;
				}
			}
		}
		return (best < 0) ? 3 : best;
	}

	public double[,] GetGridData()
	{
		int num = dataGridView1.RowCount;
		int num2 = dataGridView1.ColumnCount;
		double[,] array = new double[num, num2];
		for (int i = 1; i < num; i++)
		{
			for (int j = 1; j < num2; j++)
			{
				object value = dataGridView1[j, i].Value;
				if (value != null)
				{
					if (double.TryParse(value.ToString(), out var result))
					{
						array[i, j] = result;
					}
					else
					{
						array[i, j] = 0.0;
					}
				}
				else
				{
					array[i, j] = 0.0;
				}
			}
		}
		return array;
	}

	private void TPS2Values()
	{
		int num = rowCount + 1;
		int num2 = columnCount;
		dataGridView1.Rows.Clear();
		dataGridView1.Columns.Clear();
		for (int i = 0; i < num2 + 1; i++)
		{
			dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
		}
		for (int j = 0; j < num; j++)
		{
			dataGridView1.Rows.Add();
		}
		if (selectedStartAddress == 238168 || selectedStartAddress == 239620 || selectedStartAddress == 233756 || customBigEndian)
		{
			for (int k = 1; k < num2 + 1; k++)
			{
				int num3 = tpsh + (k - 1) * 2;
				if (num3 + 1 < binBytes.Length)
				{
					ushort num4 = BitConverter.ToUInt16(new byte[2]
					{
						binBytes[num3 + 1],
						binBytes[num3]
					}, 0);
					dataGridView1[k, 0].Value = ((double)(int)num4 / 10.0).ToString("F1");
					dataGridView1[k, 0].Style.BackColor = Color.LightGray;
					dataGridView1[k, 0].Style.ForeColor = Color.Black;
					dataGridView1[k, 0].ReadOnly = true;
				}
				else
				{
					dataGridView1[k, 0].Value = "N/A";
					dataGridView1[k, 0].Style.BackColor = Color.LightGray;
					dataGridView1[k, 0].Style.ForeColor = Color.Black;
					dataGridView1[k, 0].ReadOnly = true;
				}
			}
			for (int l = 1; l < num; l++)
			{
				if (l >= dataGridView1.RowCount)
				{
					dataGridView1.Rows.Add();
				}
				if (dataGridView1.ColumnCount > 0)
				{
					int num5 = rpmh + (l - 1) * 2;
					if (num5 + 1 < binBytes.Length)
					{
						ushort num6 = BitConverter.ToUInt16(new byte[2]
						{
							binBytes[num5 + 1],
							binBytes[num5]
						}, 0);
						dataGridView1[0, l].Value = ((float)(int)num6 / kon).ToString("F0");
						dataGridView1[0, l].Style.BackColor = Color.LightGray;
						dataGridView1[0, l].Style.ForeColor = Color.Black;
						dataGridView1[0, l].ReadOnly = true;
					}
					else
					{
						dataGridView1[0, l].Value = "N/A";
						dataGridView1[0, l].Style.BackColor = Color.LightGray;
						dataGridView1[0, l].Style.ForeColor = Color.Black;
						dataGridView1[0, l].ReadOnly = true;
					}
				}
			}
			minValue = ushort.MaxValue;
			maxValue = 0;
			for (int m = 1; m < num; m++)
			{
				if (m >= dataGridView1.RowCount)
				{
					continue;
				}
				for (int n = 1; n < num2 + 1; n++)
				{
					if (n < dataGridView1.ColumnCount)
					{
						int num7 = selectedStartAddress + ((m - 1) * num2 + (n - 1)) * 2;
						if (num7 + 1 < binBytes.Length && num7 + 1 < binBytesORI.Length)
						{
							ushort num8 = BitConverter.ToUInt16(new byte[2]
							{
								binBytes[num7 + 1],
								binBytes[num7]
							}, 0);
							ushort num9 = BitConverter.ToUInt16(new byte[2]
							{
								binBytesORI[num7 + 1],
								binBytesORI[num7]
							}, 0);
							if (isCompared)
							{
								if (num8 < minValue)
								{
									minValue = num8;
								}
								if (num8 > maxValue)
								{
									maxValue = num8;
								}
								double num10 = (double)(num8 - num9) / 1000.0;
								Debug.WriteLine($"Assigning to difference[{n}, {m}] = {num10}");
								string value = FormatCompareText((double)num8 / 1000.0, (double)num9 / 1000.0, "+0.000;-0.000;0.000");
								dataGridView1[n, m].Value = value;
							}
							else
							{
								if (num8 < minValue)
								{
									minValue = num8;
								}
								if (num8 > maxValue)
								{
									maxValue = num8;
								}
								double num11 = (double)(int)num8 / 1000.0;
								Debug.WriteLine($"Assigning to DataGridView[{n}, {m}] = {num11}");
								dataGridView1[n, m].Value = num11.ToString("F3");
							}
						}
						else
						{
							dataGridView1[n, m].Value = "N/A";
						}
					}
					else
					{
						dataGridView1[n, m].Value = "N/A";
					}
				}
			}
		}
		else
		{
			for (int num12 = 1; num12 < num2 + 1; num12++)
			{
				int num13 = tpsh + (num12 - 1) * 2;
				if (num13 + 1 < binBytes.Length)
				{
					ushort num14 = BitConverter.ToUInt16(new byte[2]
					{
						binBytes[num13],
						binBytes[num13 + 1]
					}, 0);
					dataGridView1[num12, 0].Value = ((double)(int)num14 / 10.0).ToString("F1");
					dataGridView1[num12, 0].Style.BackColor = Color.LightGray;
					dataGridView1[num12, 0].Style.ForeColor = Color.Black;
					dataGridView1[num12, 0].ReadOnly = true;
				}
				else
				{
					dataGridView1[num12, 0].Value = "N/A";
					dataGridView1[num12, 0].Style.BackColor = Color.LightGray;
					dataGridView1[num12, 0].Style.ForeColor = Color.Black;
					dataGridView1[num12, 0].ReadOnly = true;
				}
			}
			for (int num15 = 1; num15 < num; num15++)
			{
				if (num15 >= dataGridView1.RowCount)
				{
					dataGridView1.Rows.Add();
				}
				if (dataGridView1.ColumnCount > 0)
				{
					int num16 = rpmh + (num15 - 1) * 2;
					if (num16 + 1 < binBytes.Length)
					{
						ushort num17 = BitConverter.ToUInt16(new byte[2]
						{
							binBytes[num16],
							binBytes[num16 + 1]
						}, 0);
						dataGridView1[0, num15].Value = ((float)(int)num17 / kon).ToString("F0");
						dataGridView1[0, num15].Style.BackColor = Color.LightGray;
						dataGridView1[0, num15].Style.ForeColor = Color.Black;
						dataGridView1[0, num15].ReadOnly = true;
					}
					else
					{
						dataGridView1[0, num15].Value = "N/A";
						dataGridView1[0, num15].Style.BackColor = Color.LightGray;
						dataGridView1[0, num15].Style.ForeColor = Color.Black;
						dataGridView1[0, num15].ReadOnly = true;
					}
				}
			}
			minValue = ushort.MaxValue;
			maxValue = 0;
			for (int num18 = 1; num18 < num; num18++)
			{
				if (num18 >= dataGridView1.RowCount)
				{
					continue;
				}
				for (int num19 = 1; num19 < num2 + 1; num19++)
				{
					if (num19 < dataGridView1.ColumnCount)
					{
						int num20 = selectedStartAddress + ((num18 - 1) * num2 + (num19 - 1)) * 2;
						if (num20 + 1 < binBytes.Length && num20 + 1 < binBytesORI.Length)
						{
							ushort num21 = BitConverter.ToUInt16(new byte[2]
							{
								binBytes[num20],
								binBytes[num20 + 1]
							}, 0);
							ushort num22 = BitConverter.ToUInt16(new byte[2]
							{
								binBytesORI[num20],
								binBytesORI[num20 + 1]
							}, 0);
							if (isCompared)
							{
								if (num21 < minValue)
								{
									minValue = num21;
								}
								if (num21 > maxValue)
								{
									maxValue = num21;
								}
								double num23 = (double)(num21 - num22) / 1000.0;
								Debug.WriteLine($"Assigning to difference[{num19}, {num18}] = {num23}");
								dataGridView1[num19, num18].Value = num23.ToString("F3");
								string value2 = FormatCompareText((double)num21 / 1000.0, (double)num22 / 1000.0, "+0.000;-0.000;0.000");
								dataGridView1[num19, num18].Value = value2;
							}
							else
							{
								if (num21 < minValue)
								{
									minValue = num21;
								}
								if (num21 > maxValue)
								{
									maxValue = num21;
								}
								double num24 = (double)(int)num21 / 1000.0;
								Debug.WriteLine($"Assigning to DataGridView[{num19}, {num18}] = {num24}");
								dataGridView1[num19, num18].Value = num24.ToString("F3");
							}
						}
						else
						{
							dataGridView1[num19, num18].Value = "N/A";
						}
					}
					else
					{
						dataGridView1[num19, num18].Value = "N/A";
					}
				}
			}
		}
		ukuran();
	}

	private void CompareValues()
	{
		int num = dataGridView1.RowCount;
		int num2 = dataGridView1.ColumnCount - 1;
		for (int i = 1; i < num; i++)
		{
			for (int j = 1; j <= num2; j++)
			{
				int num3 = feul1 + ((i - 1) * num2 + (j - 1)) * 2;
				if (num3 + 1 < binBytes.Length && num3 + 1 < binBytesORI.Length)
				{
					ushort num4 = BitConverter.ToUInt16(new byte[2]
					{
						binBytes[num3],
						binBytes[num3 + 1]
					}, 0);
					ushort num5 = BitConverter.ToUInt16(new byte[2]
					{
						binBytesORI[num3],
						binBytesORI[num3 + 1]
					}, 0);
					double num6 = (double)(int)num4 / 1000.0;
					double num7 = (double)(int)num5 / 1000.0;
					double num8 = num6 - num7;
					string value = FormatCompareText(num6, num7, "+0.000;-0.000;0.000");
					dataGridView1[j, i].Value = value;
				}
				else
				{
					dataGridView1[j, i].Value = "N/A";
				}
			}
		}
		dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
		dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
	}

	private void ResetValues()
	{
		int num = dataGridView1.RowCount;
		int num2 = dataGridView1.ColumnCount - 1;
		for (int i = 1; i < num; i++)
		{
			for (int j = 1; j <= num2; j++)
			{
				int num3 = feul1 + ((i - 1) * num2 + (j - 1)) * 2;
				if (num3 + 1 < binBytes.Length)
				{
					ushort num4 = BitConverter.ToUInt16(new byte[2]
					{
						binBytes[num3],
						binBytes[num3 + 1]
					}, 0);
					double num5 = (double)(int)num4 / 1000.0;
					dataGridView1[j, i].Value = num5.ToString("F3");
				}
				else
				{
					dataGridView1[j, i].Value = "N/A";
				}
			}
		}
		dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
		dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
	}

	private void SaveEditedValues()
	{
		int num = rowCount + 1;
		int num2 = columnCount;
		if (selectedStartAddress == feul2 || selectedStartAddress == feul1 || selectedStartAddress == feul3 || selectedStartAddress == iap)
		{
			if (selectedStartAddress == 239620 || selectedStartAddress == 238168 || selectedStartAddress == 233756 || selectedStartAddress == 235208 || customBigEndian)
			{
				for (int i = 1; i < num; i++)
				{
					for (int j = 1; j < num2 + 1; j++)
					{
						int num3 = selectedStartAddress + ((i - 1) * num2 + (j - 1)) * 2;
						if (num3 + 1 < binBytes.Length && double.TryParse(dataGridView1[j, i].Value.ToString(), out var result))
						{
							ushort num4 = ClampU16(result * 1000.0);
							binBytes[num3] = (byte)((num4 >> 8) & 0xFF);
							binBytes[num3 + 1] = (byte)(num4 & 0xFF);
						}
					}
				}
			}
			else
			{
				for (int k = 1; k < num; k++)
				{
					for (int l = 1; l < num2 + 1; l++)
					{
						int num5 = selectedStartAddress + ((k - 1) * num2 + (l - 1)) * 2;
						if (num5 + 1 < binBytes.Length && double.TryParse(dataGridView1[l, k].Value.ToString(), out var result2))
						{
							ushort num6 = ClampU16(result2 * 1000.0);
							binBytes[num5] = (byte)(num6 & 0xFF);
							binBytes[num5 + 1] = (byte)((num6 >> 8) & 0xFF);
						}
					}
				}
			}
		}
		else if (selectedStartAddress == ig1 || selectedStartAddress == ig2 || selectedStartAddress == ig3 || selectedStartAddress == ig4)
		{
			if (selectedStartAddress == 244144 || selectedStartAddress == 238572)
			{
				for (int m = 1; m < num; m++)
				{
					for (int n = 1; n < num2 + 1; n++)
					{
						int num7 = selectedStartAddress + ((m - 1) * num2 + (n - 1)) * 2;
						if (num7 + 1 < binBytes.Length && double.TryParse(dataGridView1[n, m].Value.ToString(), out var result3))
						{
							ushort num8 = (ushort)Math.Round((result3 + 11.53) / 19.827 * 255.0);
							binBytes[num7] = (byte)(num8 >> 8);
							binBytes[num7 + 1] = (byte)(num8 & 0xFF);
						}
					}
				}
			}
			else if (selectedStartAddress == 210634 || selectedStartAddress == 219134)
			{
				for (int num9 = 1; num9 < num; num9++)
				{
					for (int num10 = 1; num10 < num2 + 1; num10++)
					{
						int num11 = selectedStartAddress + ((num9 - 1) * num2 + (num10 - 1)) * 2;
						if (num11 + 1 < binBytes.Length && double.TryParse(dataGridView1[num10, num9].Value.ToString(), out var result4))
						{
							ushort num12 = ConvertValueToUShort(result4);
							binBytes[num11] = (byte)(num12 & 0xFF);
							binBytes[num11 + 1] = (byte)((num12 >> 8) & 0xFF);
						}
					}
				}
			}
			else
			{
				for (int num13 = 1; num13 < num; num13++)
				{
					for (int num14 = 1; num14 < num2 + 1; num14++)
					{
						int num15 = selectedStartAddress + ((num13 - 1) * num2 + (num14 - 1));
						if (num15 < binBytes.Length && double.TryParse(dataGridView1[num14, num13].Value.ToString(), out var result5))
						{
							byte b = ConvertValueToByte(result5);
							binBytes[num15] = b;
						}
					}
				}
			}
		}
		else if (selectedStartAddress == tpsh)
		{
			int num16 = 1;
			if (selectedStartAddress == 238056 || selectedStartAddress == 233644)
			{
				for (int num17 = 1; num17 <= columnCount; num17++)
				{
					if (num17 >= dataGridView1.ColumnCount || num16 >= dataGridView1.RowCount)
					{
						continue;
					}
					string text = dataGridView1[num17, num16].Value?.ToString();
					if (!string.IsNullOrEmpty(text) && text != "N/A")
					{
						double num18 = double.Parse(text);
						ushort value = ClampU16(num18 * 10.0);
						int num19 = selectedStartAddress + (num16 - 1) * columnCount * 2 + (num17 - 1) * 2;
						if (num19 + 1 < binBytes.Length)
						{
							byte[] bytes = BitConverter.GetBytes(value);
							binBytes[num19] = bytes[1];
							binBytes[num19 + 1] = bytes[0];
						}
					}
				}
			}
			else
			{
				for (int num20 = 1; num20 <= columnCount; num20++)
				{
					if (num20 >= dataGridView1.ColumnCount || num16 >= dataGridView1.RowCount)
					{
						continue;
					}
					string text2 = dataGridView1[num20, num16].Value.ToString();
					if (!string.IsNullOrEmpty(text2) && text2 != "N/A")
					{
						double num21 = double.Parse(text2);
						ushort value2 = ClampU16(num21 * 10.0);
						int num22 = selectedStartAddress + (num16 - 1) * columnCount * 2 + (num20 - 1) * 2;
						if (num22 + 1 < binBytes.Length)
						{
							byte[] bytes2 = BitConverter.GetBytes(value2);
							binBytes[num22] = bytes2[0];
							binBytes[num22 + 1] = bytes2[1];
						}
					}
				}
			}
		}
		else if (selectedStartAddress == rpmh)
		{
			num2 = 2;
			num = rowCount + 1;
			if (selectedStartAddress == 238108 || selectedStartAddress == 233696)
			{
				for (int num23 = 1; num23 < num; num23++)
				{
					if (num23 >= dataGridView1.RowCount || dataGridView1.ColumnCount < 2)
					{
						continue;
					}
					string text3 = dataGridView1[1, num23].Value?.ToString();
					if (string.IsNullOrEmpty(text3) || !(text3 != "N/A"))
					{
						continue;
					}
					if (double.TryParse(text3, out var result6))
					{
						ushort num24 = (ushort)Math.Round(result6 * (double)kon);
						int num25 = selectedStartAddress + (num23 - 1) * 2;
						if (num25 + 1 < binBytes.Length)
						{
							binBytes[num25] = (byte)((num24 >> 8) & 0xFF);
							binBytes[num25 + 1] = (byte)(num24 & 0xFF);
						}
						else
						{
							Debug.WriteLine($"Index {num25} is out of range.");
						}
					}
					else
					{
						Debug.WriteLine("Failed to parse cell value '" + text3 + "' to double.");
					}
				}
			}
			else
			{
				for (int num26 = 1; num26 < num; num26++)
				{
					if (num26 >= dataGridView1.RowCount || dataGridView1.ColumnCount < num2)
					{
						continue;
					}
					string text4 = dataGridView1[1, num26].Value.ToString();
					if (!string.IsNullOrEmpty(text4) && text4 != "N/A")
					{
						double num27 = double.Parse(text4);
						ushort value3 = ClampU16(num27 * (double)kon);
						int num28 = selectedStartAddress + (num26 - 1) * 2;
						if (num28 + 1 < binBytes.Length)
						{
							byte[] bytes3 = BitConverter.GetBytes(value3);
							binBytes[num28] = bytes3[0];
							binBytes[num28 + 1] = bytes3[1];
						}
					}
				}
			}
		}
		else if (selectedStartAddress == limit1 || selectedStartAddress == limit2 || selectedStartAddress == limit3 || selectedStartAddress == limit4 || selectedStartAddress == limit5 || selectedStartAddress == limit6 || selectedStartAddress == limit7)
		{
			num = 2;
			num2 = 3;
			int num29 = 2;
			int num30 = num29;
			if (selectedStartAddress == 105844 || selectedStartAddress == 234008 || selectedStartAddress == 105840 || selectedStartAddress == 106028 || selectedStartAddress == 239940 || selectedStartAddress == 240602 || selectedStartAddress == 245100 || selectedStartAddress == 105744)
			{
				num30 = 4;
			}
			if (selectedStartAddress == 9260 || selectedStartAddress == 21032 || selectedStartAddress == 8433 || selectedStartAddress == 3456 || selectedStartAddress == 3182 || selectedStartAddress == 20382 || selectedStartAddress == 38787 || selectedStartAddress == 46933 || selectedStartAddress == 47015 || selectedStartAddress == 47466 || selectedStartAddress == 46523 || selectedStartAddress == 46744 || selectedStartAddress == 20290 || selectedStartAddress == 38788 || selectedStartAddress == 38627 || selectedStartAddress == 20282 || selectedStartAddress == 47429 || selectedStartAddress == 38758 || selectedStartAddress == 46978 || selectedStartAddress == 47493 || selectedStartAddress == 5275 || selectedStartAddress == 47042 || selectedStartAddress == 38824 || selectedStartAddress == 16074 || selectedStartAddress == 45122 || selectedStartAddress == 38646 || selectedStartAddress == 44295 || selectedStartAddress == 16048 || selectedStartAddress == 38530 || selectedStartAddress == 44179 || selectedStartAddress == 45006 || selectedStartAddress == 15959 || selectedStartAddress == 45219 || selectedStartAddress == 38628 || selectedStartAddress == 44386 || selectedStartAddress == 38878 || selectedStartAddress == 47105 || selectedStartAddress == 47556 || selectedStartAddress == 7342 || selectedStartAddress == 44395 || selectedStartAddress == 45228 || selectedStartAddress == 8215 || selectedStartAddress == 44541 || selectedStartAddress == 10320 || selectedStartAddress == 45374 || selectedStartAddress == 38734 || selectedStartAddress == 22286 || selectedStartAddress == 9493 || selectedStartAddress == 3588 || selectedStartAddress == 44404 || selectedStartAddress == 45237 || selectedStartAddress == 16072 || selectedStartAddress == 44330 || selectedStartAddress == 45163)
			{
				num30 = 10;
			}
			if (selectedStartAddress == 43901)
			{
				num30 = 40;
			}
			if (selectedStartAddress == 47036 || selectedStartAddress == 47100)
			{
				num30 = 16;
			}
			if (selectedStartAddress == 13436)
			{
				num30 = 18;
			}
			if (selectedStartAddress == 235344 || selectedStartAddress == 235684 || selectedStartAddress == 235836 || selectedStartAddress == 236008 || selectedStartAddress == 230932 || selectedStartAddress == 231272 || selectedStartAddress == 231424 || selectedStartAddress == 231596 || selectedStartAddress == 237002 || selectedStartAddress == 241414)
			{
				for (int num31 = 1; num31 < num; num31++)
				{
					for (int num32 = 1; num32 < num2; num32++)
					{
						int num33 = selectedStartAddress + (num31 - 1) * num2 * num29 + (num32 - 1) * num30;
						if (num33 + 1 < binBytes.Length)
						{
							if (ushort.TryParse(dataGridView1[num32, num31].Value?.ToString(), out var result7))
							{
								ushort num34 = (ushort)((float)(int)result7 * lm);
								binBytes[num33] = (byte)((num34 >> 8) & 0xFF);
								binBytes[num33 + 1] = (byte)(num34 & 0xFF);
							}
							else
							{
								MessageBox.Show($"Invalid value at row {num31}, column {num32}");
							}
							continue;
						}
						MessageBox.Show($"Index out of bounds at row {num31}, column {num32}. Could not write to binBytes.");
						return;
					}
				}
			}
			else
			{
				for (int num35 = 1; num35 < num; num35++)
				{
					for (int num36 = 1; num36 < num2; num36++)
					{
						int num37 = selectedStartAddress + (num35 - 1) * num2 * num29 + (num36 - 1) * num30;
						if (num37 + 1 < binBytes.Length)
						{
							if (ushort.TryParse(dataGridView1[num36, num35].Value?.ToString(), out var result8))
							{
								ushort num38 = (ushort)((float)(int)result8 * lm);
								binBytes[num37] = (byte)(num38 & 0xFF);
								binBytes[num37 + 1] = (byte)((num38 >> 8) & 0xFF);
							}
							else
							{
								MessageBox.Show($"Invalid value at row {num35}, column {num36}");
							}
							continue;
						}
						MessageBox.Show($"Index out of bounds at row {num35}, column {num36}. Could not write to binBytes.");
						return;
					}
				}
			}
		}
		else if (selectedStartAddress == iaph)
		{
			int num39 = 1;
			for (int num40 = 1; num40 <= columnCount; num40++)
			{
				if (num40 < dataGridView1.ColumnCount && num39 < dataGridView1.RowCount)
				{
					int num41 = selectedStartAddress + ((num39 - 1) * num39 + (num40 - 1));
					if (num41 < binBytes.Length && double.TryParse(dataGridView1[num40, num39].Value.ToString(), out var result9))
					{
						byte b2 = (byte)(result9 * 2.514);
						binBytes[num41] = b2;
					}
				}
			}
		}
		MessageBox.Show("Nilai Sudah Tersave", "Save Data", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		static double ConvertUShortToValue(ushort num42)
		{
			return (double)(int)num42 / 255.0 * 19.827 - 11.53;
		}
		static ushort ConvertValueToUShort(double num43)
		{
			double num42 = ConvertUShortToValue(ushort.MaxValue);
			if (num43 < -11.53)
			{
				num43 = -11.53;
			}
			else if (num43 > num42)
			{
				num43 = num42;
			}
			return (ushort)Math.Round((num43 + 11.53) / 19.827 * 255.0);
		}
	}

	private void SaveEditedValues2()
	{
		int num = rowCount + 1;
		int num2 = columnCount;
		if (selectedStartAddress == feul2 || selectedStartAddress == feul1 || selectedStartAddress == feul3 || selectedStartAddress == iap)
		{
			if (selectedStartAddress == 239620 || selectedStartAddress == 238168 || selectedStartAddress == 233756 || selectedStartAddress == 235208 || customBigEndian)
			{
				for (int i = 1; i < num; i++)
				{
					for (int j = 1; j < num2 + 1; j++)
					{
						int num3 = selectedStartAddress + ((i - 1) * num2 + (j - 1)) * 2;
						if (num3 + 1 < binBytes.Length && double.TryParse(dataGridView1[j, i].Value.ToString(), out var result))
						{
							ushort num4 = ClampU16(result * 1000.0);
							binBytes[num3] = (byte)((num4 >> 8) & 0xFF);
							binBytes[num3 + 1] = (byte)(num4 & 0xFF);
						}
					}
				}
				return;
			}
			for (int k = 1; k < num; k++)
			{
				for (int l = 1; l < num2 + 1; l++)
				{
					int num5 = selectedStartAddress + ((k - 1) * num2 + (l - 1)) * 2;
					if (num5 + 1 < binBytes.Length && double.TryParse(dataGridView1[l, k].Value.ToString(), out var result2))
					{
						ushort num6 = ClampU16(result2 * 1000.0);
						binBytes[num5] = (byte)(num6 & 0xFF);
						binBytes[num5 + 1] = (byte)((num6 >> 8) & 0xFF);
					}
				}
			}
		}
		else if (selectedStartAddress == ig1 || selectedStartAddress == ig2 || selectedStartAddress == ig3 || selectedStartAddress == ig4)
		{
			if (selectedStartAddress == 244144 || selectedStartAddress == 238572)
			{
				for (int m = 1; m < num; m++)
				{
					for (int n = 1; n < num2 + 1; n++)
					{
						int num7 = selectedStartAddress + ((m - 1) * num2 + (n - 1)) * 2;
						if (num7 + 1 < binBytes.Length && double.TryParse(dataGridView1[n, m].Value.ToString(), out var result3))
						{
							ushort num8 = (ushort)Math.Round((result3 + 11.53) / 19.827 * 255.0);
							binBytes[num7] = (byte)(num8 >> 8);
							binBytes[num7 + 1] = (byte)(num8 & 0xFF);
						}
					}
				}
				return;
			}
			if (selectedStartAddress == 210634 || selectedStartAddress == 219134)
			{
				for (int num9 = 1; num9 < num; num9++)
				{
					for (int num10 = 1; num10 < num2 + 1; num10++)
					{
						int num11 = selectedStartAddress + ((num9 - 1) * num2 + (num10 - 1)) * 2;
						if (num11 + 1 < binBytes.Length && double.TryParse(dataGridView1[num10, num9].Value.ToString(), out var result4))
						{
							ushort num12 = ConvertValueToUShort(result4);
							binBytes[num11] = (byte)(num12 & 0xFF);
							binBytes[num11 + 1] = (byte)((num12 >> 8) & 0xFF);
						}
					}
				}
				return;
			}
			for (int num13 = 1; num13 < num; num13++)
			{
				for (int num14 = 1; num14 < num2 + 1; num14++)
				{
					int num15 = selectedStartAddress + ((num13 - 1) * num2 + (num14 - 1));
					if (num15 < binBytes.Length && double.TryParse(dataGridView1[num14, num13].Value.ToString(), out var result5))
					{
						byte b = ConvertValueToByte(result5);
						binBytes[num15] = b;
					}
				}
			}
		}
		else if (selectedStartAddress == tpsh)
		{
			int num16 = 1;
			if (selectedStartAddress == 238056 || selectedStartAddress == 233644)
			{
				for (int num17 = 1; num17 <= columnCount; num17++)
				{
					if (num17 >= dataGridView1.ColumnCount || num16 >= dataGridView1.RowCount)
					{
						continue;
					}
					string text = dataGridView1[num17, num16].Value?.ToString();
					if (!string.IsNullOrEmpty(text) && text != "N/A")
					{
						double num18 = double.Parse(text);
						ushort value = ClampU16(num18 * 10.0);
						int num19 = selectedStartAddress + (num16 - 1) * columnCount * 2 + (num17 - 1) * 2;
						if (num19 + 1 < binBytes.Length)
						{
							byte[] bytes = BitConverter.GetBytes(value);
							binBytes[num19] = bytes[1];
							binBytes[num19 + 1] = bytes[0];
						}
					}
				}
				return;
			}
			for (int num20 = 1; num20 <= columnCount; num20++)
			{
				if (num20 >= dataGridView1.ColumnCount || num16 >= dataGridView1.RowCount)
				{
					continue;
				}
				string text2 = dataGridView1[num20, num16].Value.ToString();
				if (!string.IsNullOrEmpty(text2) && text2 != "N/A")
				{
					double num21 = double.Parse(text2);
					ushort value2 = ClampU16(num21 * 10.0);
					int num22 = selectedStartAddress + (num16 - 1) * columnCount * 2 + (num20 - 1) * 2;
					if (num22 + 1 < binBytes.Length)
					{
						byte[] bytes2 = BitConverter.GetBytes(value2);
						binBytes[num22] = bytes2[0];
						binBytes[num22 + 1] = bytes2[1];
					}
				}
			}
		}
		else if (selectedStartAddress == rpmh)
		{
			num2 = 2;
			num = rowCount + 1;
			if (selectedStartAddress == 238108 || selectedStartAddress == 233696)
			{
				for (int num23 = 1; num23 < num; num23++)
				{
					if (num23 >= dataGridView1.RowCount || dataGridView1.ColumnCount < 2)
					{
						continue;
					}
					string text3 = dataGridView1[1, num23].Value?.ToString();
					if (string.IsNullOrEmpty(text3) || !(text3 != "N/A"))
					{
						continue;
					}
					if (double.TryParse(text3, out var result6))
					{
						ushort num24 = (ushort)Math.Round(result6 * (double)kon);
						int num25 = selectedStartAddress + (num23 - 1) * 2;
						if (num25 + 1 < binBytes.Length)
						{
							binBytes[num25] = (byte)((num24 >> 8) & 0xFF);
							binBytes[num25 + 1] = (byte)(num24 & 0xFF);
						}
						else
						{
							Debug.WriteLine($"Index {num25} is out of range.");
						}
					}
					else
					{
						Debug.WriteLine("Failed to parse cell value '" + text3 + "' to double.");
					}
				}
				return;
			}
			for (int num26 = 1; num26 < num; num26++)
			{
				if (num26 >= dataGridView1.RowCount || dataGridView1.ColumnCount < num2)
				{
					continue;
				}
				string text4 = dataGridView1[1, num26].Value.ToString();
				if (!string.IsNullOrEmpty(text4) && text4 != "N/A")
				{
					double num27 = double.Parse(text4);
					ushort value3 = ClampU16(num27 * (double)kon);
					int num28 = selectedStartAddress + (num26 - 1) * 2;
					if (num28 + 1 < binBytes.Length)
					{
						byte[] bytes3 = BitConverter.GetBytes(value3);
						binBytes[num28] = bytes3[0];
						binBytes[num28 + 1] = bytes3[1];
					}
				}
			}
		}
		else if (selectedStartAddress == limit1 || selectedStartAddress == limit2 || selectedStartAddress == limit3 || selectedStartAddress == limit4 || selectedStartAddress == limit5 || selectedStartAddress == limit6 || selectedStartAddress == limit7)
		{
			num = 2;
			num2 = 3;
			int num29 = 2;
			int num30 = num29;
			if (selectedStartAddress == 105844 || selectedStartAddress == 234008 || selectedStartAddress == 105840 || selectedStartAddress == 106028 || selectedStartAddress == 239940 || selectedStartAddress == 240602 || selectedStartAddress == 245100 || selectedStartAddress == 105744)
			{
				num30 = 4;
			}
			if (selectedStartAddress == 9260 || selectedStartAddress == 21032 || selectedStartAddress == 8433 || selectedStartAddress == 3456 || selectedStartAddress == 3182 || selectedStartAddress == 20382 || selectedStartAddress == 38787 || selectedStartAddress == 46933 || selectedStartAddress == 47015 || selectedStartAddress == 47466 || selectedStartAddress == 46523 || selectedStartAddress == 46744 || selectedStartAddress == 20290 || selectedStartAddress == 38788 || selectedStartAddress == 38627 || selectedStartAddress == 20282 || selectedStartAddress == 47429 || selectedStartAddress == 38758 || selectedStartAddress == 46978 || selectedStartAddress == 47493 || selectedStartAddress == 47042 || selectedStartAddress == 38824 || selectedStartAddress == 16074 || selectedStartAddress == 45122 || selectedStartAddress == 38646 || selectedStartAddress == 44295 || selectedStartAddress == 16048 || selectedStartAddress == 38530 || selectedStartAddress == 44179 || selectedStartAddress == 45006 || selectedStartAddress == 15959 || selectedStartAddress == 45219 || selectedStartAddress == 38628 || selectedStartAddress == 44386 || selectedStartAddress == 38878 || selectedStartAddress == 47105 || selectedStartAddress == 47556 || selectedStartAddress == 7342 || selectedStartAddress == 44395 || selectedStartAddress == 45228 || selectedStartAddress == 8215 || selectedStartAddress == 44541 || selectedStartAddress == 10320 || selectedStartAddress == 45374 || selectedStartAddress == 38734 || selectedStartAddress == 22286 || selectedStartAddress == 9493 || selectedStartAddress == 3588 || selectedStartAddress == 44404 || selectedStartAddress == 45237 || selectedStartAddress == 16072 || selectedStartAddress == 44330 || selectedStartAddress == 45163)
			{
				num30 = 10;
			}
			if (selectedStartAddress == 43901)
			{
				num30 = 40;
			}
			if (selectedStartAddress == 47036 || selectedStartAddress == 47100)
			{
				num30 = 16;
			}
			if (selectedStartAddress == 13436)
			{
				num30 = 18;
			}
			if (selectedStartAddress == 235344 || selectedStartAddress == 235684 || selectedStartAddress == 235836 || selectedStartAddress == 236008 || selectedStartAddress == 230932 || selectedStartAddress == 231272 || selectedStartAddress == 231424 || selectedStartAddress == 231596 || selectedStartAddress == 237002 || selectedStartAddress == 241414)
			{
				for (int num31 = 1; num31 < num; num31++)
				{
					for (int num32 = 1; num32 < num2; num32++)
					{
						int num33 = selectedStartAddress + (num31 - 1) * num2 * num29 + (num32 - 1) * num30;
						if (num33 + 1 < binBytes.Length)
						{
							if (ushort.TryParse(dataGridView1[num32, num31].Value?.ToString(), out var result7))
							{
								ushort num34 = (ushort)((float)(int)result7 * lm);
								binBytes[num33] = (byte)((num34 >> 8) & 0xFF);
								binBytes[num33 + 1] = (byte)(num34 & 0xFF);
							}
							else
							{
								MessageBox.Show($"Invalid value at row {num31}, column {num32}");
							}
							continue;
						}
						MessageBox.Show($"Index out of bounds at row {num31}, column {num32}. Could not write to binBytes.");
						return;
					}
				}
				return;
			}
			for (int num35 = 1; num35 < num; num35++)
			{
				for (int num36 = 1; num36 < num2; num36++)
				{
					int num37 = selectedStartAddress + (num35 - 1) * num2 * num29 + (num36 - 1) * num30;
					if (num37 + 1 < binBytes.Length)
					{
						if (ushort.TryParse(dataGridView1[num36, num35].Value?.ToString(), out var result8))
						{
							ushort num38 = (ushort)((float)(int)result8 * lm);
							binBytes[num37] = (byte)(num38 & 0xFF);
							binBytes[num37 + 1] = (byte)((num38 >> 8) & 0xFF);
						}
						else
						{
							MessageBox.Show($"Invalid value at row {num35}, column {num36}");
						}
						continue;
					}
					MessageBox.Show($"Index out of bounds at row {num35}, column {num36}. Could not write to binBytes.");
					return;
				}
			}
		}
		else
		{
			if (selectedStartAddress != iaph)
			{
				return;
			}
			int num39 = 1;
			for (int num40 = 1; num40 <= columnCount; num40++)
			{
				if (num40 < dataGridView1.ColumnCount && num39 < dataGridView1.RowCount)
				{
					int num41 = selectedStartAddress + ((num39 - 1) * num39 + (num40 - 1));
					if (num41 < binBytes.Length && double.TryParse(dataGridView1[num40, num39].Value.ToString(), out var result9))
					{
						byte b2 = (byte)(result9 * 2.514);
						binBytes[num41] = b2;
					}
				}
			}
		}
		static double ConvertUShortToValue(ushort num42)
		{
			return (double)(int)num42 / 255.0 * 19.827 - 11.53;
		}
		static ushort ConvertValueToUShort(double num43)
		{
			double num42 = ConvertUShortToValue(ushort.MaxValue);
			if (num43 < -11.53)
			{
				num43 = -11.53;
			}
			else if (num43 > num42)
			{
				num43 = num42;
			}
			return (ushort)Math.Round((num43 + 11.53) / 19.827 * 255.0);
		}
	}

	private void TPS3Values()
	{
		int num = rowCount + 1;
		int num2 = columnCount;
		while (dataGridView1.RowCount < num)
		{
			dataGridView1.Rows.Add();
		}
		while (dataGridView1.ColumnCount < num2 + 1)
		{
			dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
		}
		if (selectedStartAddress == 238168 || selectedStartAddress == 235208)
		{
			for (int i = 1; i < num2 + 1; i++)
			{
				int num3 = tpsh + (i - 1) * 2;
				if (num3 + 1 < binBytes.Length)
				{
					ushort num4 = BitConverter.ToUInt16(new byte[2]
					{
						binBytes[num3 + 1],
						binBytes[num3]
					}, 0);
					dataGridView1[i, 0].Value = ((double)(int)num4 / 10.0).ToString("F1");
					dataGridView1[i, 0].Style.BackColor = Color.LightGray;
					dataGridView1[i, 0].Style.ForeColor = Color.Black;
					dataGridView1[i, 0].ReadOnly = true;
				}
				else
				{
					dataGridView1[i, 0].Value = "N/A";
					dataGridView1[i, 0].Style.BackColor = Color.LightGray;
					dataGridView1[i, 0].Style.ForeColor = Color.Black;
					dataGridView1[i, 0].ReadOnly = true;
				}
			}
			for (int j = 1; j < num; j++)
			{
				if (j >= dataGridView1.RowCount)
				{
					dataGridView1.Rows.Add();
				}
				if (dataGridView1.ColumnCount > 0)
				{
					int num5 = rpmh + (j - 1) * 2;
					if (num5 + 1 < binBytes.Length)
					{
						ushort num6 = BitConverter.ToUInt16(new byte[2]
						{
							binBytes[num5 + 1],
							binBytes[num5]
						}, 0);
						dataGridView1[0, j].Value = ((float)(int)num6 / kon).ToString("F0");
						dataGridView1[0, j].Style.BackColor = Color.LightGray;
						dataGridView1[0, j].Style.ForeColor = Color.Black;
						dataGridView1[0, j].ReadOnly = true;
					}
					else
					{
						dataGridView1[0, j].Value = "N/A";
						dataGridView1[0, j].Style.BackColor = Color.LightGray;
						dataGridView1[0, j].Style.ForeColor = Color.Black;
						dataGridView1[0, j].ReadOnly = true;
					}
				}
			}
			minValue = ushort.MaxValue;
			maxValue = 0;
			for (int k = 1; k < num; k++)
			{
				if (k >= dataGridView1.RowCount)
				{
					continue;
				}
				for (int l = 1; l < num2 + 1; l++)
				{
					if (l < dataGridView1.ColumnCount)
					{
						int num7 = selectedStartAddress + ((k - 1) * num2 + (l - 1)) * 2;
						if (num7 + 1 < binBytes.Length)
						{
							ushort num8 = BitConverter.ToUInt16(new byte[2]
							{
								binBytes[num7 + 1],
								binBytes[num7]
							}, 0);
							if (num8 < minValue)
							{
								minValue = num8;
							}
							if (num8 > maxValue)
							{
								maxValue = num8;
							}
							double num9 = (double)(int)num8 / 1000.0;
							Debug.WriteLine($"Assigning to DataGridView[{l}, {k}] = {num9}");
							dataGridView1[l, k].Value = num9.ToString("F3");
						}
						else
						{
							dataGridView1[l, k].Value = "N/A";
						}
					}
					else
					{
						dataGridView1[l, k].Value = "N/A";
					}
				}
			}
		}
		else
		{
			for (int m = 1; m < num2 + 1; m++)
			{
				int num10 = tpsh + (m - 1) * 2;
				if (num10 + 1 < binBytes.Length)
				{
					ushort num11 = BitConverter.ToUInt16(new byte[2]
					{
						binBytes[num10],
						binBytes[num10 + 1]
					}, 0);
					dataGridView1[m, 0].Value = ((double)(int)num11 / 10.0).ToString("F1");
					dataGridView1[m, 0].Style.BackColor = Color.LightGray;
					dataGridView1[m, 0].Style.ForeColor = Color.Black;
					dataGridView1[m, 0].ReadOnly = true;
				}
				else
				{
					dataGridView1[m, 0].Value = "N/A";
					dataGridView1[m, 0].Style.BackColor = Color.LightGray;
					dataGridView1[m, 0].Style.ForeColor = Color.Black;
					dataGridView1[m, 0].ReadOnly = true;
				}
			}
			for (int n = 1; n < num; n++)
			{
				if (n >= dataGridView1.RowCount)
				{
					dataGridView1.Rows.Add();
				}
				if (dataGridView1.ColumnCount > 0)
				{
					int num12 = rpmh + (n - 1) * 2;
					if (num12 + 1 < binBytes.Length)
					{
						ushort num13 = BitConverter.ToUInt16(new byte[2]
						{
							binBytes[num12],
							binBytes[num12 + 1]
						}, 0);
						dataGridView1[0, n].Value = ((float)(int)num13 / kon).ToString("F0");
						dataGridView1[0, n].Style.BackColor = Color.LightGray;
						dataGridView1[0, n].Style.ForeColor = Color.Black;
						dataGridView1[0, n].ReadOnly = true;
					}
					else
					{
						dataGridView1[0, n].Value = "N/A";
						dataGridView1[0, n].Style.BackColor = Color.LightGray;
						dataGridView1[0, n].Style.ForeColor = Color.Black;
						dataGridView1[0, n].ReadOnly = true;
					}
				}
			}
			minValue = ushort.MaxValue;
			maxValue = 0;
			for (int num14 = 1; num14 < num; num14++)
			{
				if (num14 >= dataGridView1.RowCount)
				{
					continue;
				}
				for (int num15 = 1; num15 < num2 + 1; num15++)
				{
					if (num15 < dataGridView1.ColumnCount)
					{
						int num16 = selectedStartAddress + ((num14 - 1) * num2 + (num15 - 1)) * 2;
						if (num16 + 1 < binBytes.Length)
						{
							ushort num17 = BitConverter.ToUInt16(new byte[2]
							{
								binBytes[num16],
								binBytes[num16 + 1]
							}, 0);
							if (num17 < minValue)
							{
								minValue = num17;
							}
							if (num17 > maxValue)
							{
								maxValue = num17;
							}
							double num18 = (double)(int)num17 / 1000.0;
							Debug.WriteLine($"Assigning to DataGridView[{num15}, {num14}] = {num18}");
							dataGridView1[num15, num14].Value = num18.ToString("F3");
						}
						else
						{
							dataGridView1[num15, num14].Value = "N/A";
						}
					}
					else
					{
						dataGridView1[num15, num14].Value = "N/A";
					}
				}
			}
		}
		ukuran();
	}

	private void IAPValues()
	{
		int num = rowCount + 1;
		int num2 = columnCount;
		dataGridView1.Rows.Clear();
		dataGridView1.Columns.Clear();
		for (int i = 0; i < num2 + 1; i++)
		{
			dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
		}
		for (int j = 0; j < num; j++)
		{
			dataGridView1.Rows.Add();
		}
		for (int k = 1; k < num2 + 1; k++)
		{
			int num3 = iaph + (k - 1);
			if (num3 < binBytes.Length)
			{
				byte b = binBytes[num3];
				double value = (double)(int)b / 2.514;
				dataGridView1[k, 0].Value = ((double)(int)b / 2.514).ToString("F1");
				mapValues.Add(Math.Round(value, 1));
				Debug.WriteLine($"Value saved at column {k}: {Math.Round(value, 1)}");
				dataGridView1[k, 0].Style.BackColor = Color.LightGray;
				dataGridView1[k, 0].Style.ForeColor = Color.Black;
				dataGridView1[k, 0].ReadOnly = true;
			}
			else
			{
				dataGridView1[k, 0].Value = "N/A";
				dataGridView1[k, 0].Style.BackColor = Color.LightGray;
				dataGridView1[k, 0].Style.ForeColor = Color.Black;
				dataGridView1[k, 0].ReadOnly = true;
			}
		}
		for (int l = 1; l < num; l++)
		{
			if (l >= dataGridView1.RowCount)
			{
				dataGridView1.Rows.Add();
			}
			if (dataGridView1.ColumnCount > 0)
			{
				int num4 = rpmh + (l - 1) * 2;
				if (num4 + 1 < binBytes.Length)
				{
					ushort num5 = BitConverter.ToUInt16(new byte[2]
					{
						binBytes[num4],
						binBytes[num4 + 1]
					}, 0);
					dataGridView1[0, l].Value = ((float)(int)num5 / kon).ToString("F0");
					dataGridView1[0, l].Style.BackColor = Color.LightGray;
					dataGridView1[0, l].Style.ForeColor = Color.Black;
					dataGridView1[0, l].ReadOnly = true;
				}
				else
				{
					dataGridView1[0, l].Value = "N/A";
					dataGridView1[0, l].Style.BackColor = Color.LightGray;
					dataGridView1[0, l].Style.ForeColor = Color.Black;
					dataGridView1[0, l].ReadOnly = true;
				}
			}
		}
		minValue = ushort.MaxValue;
		maxValue = 0;
		for (int m = 1; m < num; m++)
		{
			if (m >= dataGridView1.RowCount)
			{
				continue;
			}
			for (int n = 1; n < num2 + 1; n++)
			{
				if (n < dataGridView1.ColumnCount)
				{
					int num6 = selectedStartAddress + ((m - 1) * num2 + (n - 1)) * 2;
					if (num6 + 1 < binBytes.Length && num6 + 1 < binBytesORI.Length)
					{
						ushort num7 = BitConverter.ToUInt16(new byte[2]
						{
							binBytes[num6],
							binBytes[num6 + 1]
						}, 0);
						ushort num8 = BitConverter.ToUInt16(new byte[2]
						{
							binBytesORI[num6],
							binBytesORI[num6 + 1]
						}, 0);
						if (isCompared)
						{
							if (num7 < minValue)
							{
								minValue = num7;
							}
							if (num7 > maxValue)
							{
								maxValue = num7;
							}
							double num9 = (double)(num7 - num8) / 1000.0;
							string value2 = FormatCompareText((double)num7 / 1000.0, (double)num8 / 1000.0, "+0.000;-0.000;0.000");
							dataGridView1[n, m].Value = value2;
						}
						else
						{
							if (num7 < minValue)
							{
								minValue = num7;
							}
							if (num7 > maxValue)
							{
								maxValue = num7;
							}
							double num10 = (double)(int)num7 / 1000.0;
							dataGridView1[n, m].Value = num10.ToString("F3");
						}
					}
					else
					{
						dataGridView1[n, m].Value = "N/A";
					}
				}
				else
				{
					dataGridView1[n, m].Value = "N/A";
				}
			}
		}
		ukuran();
	}

	private void IGNITION1()
	{
		int num = rowCount + 1;
		int num2 = columnCount;
		dataGridView1.Rows.Clear();
		dataGridView1.Columns.Clear();
		for (int i = 0; i < num2 + 1; i++)
		{
			dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
		}
		for (int j = 0; j < num; j++)
		{
			dataGridView1.Rows.Add();
		}
		if (selectedStartAddress == 244144 || selectedStartAddress == 238572)
		{
			for (int k = 1; k < num2 + 1; k++)
			{
				int num3 = tpsh + (k - 1) * 2;
				if (num3 + 1 < binBytes.Length)
				{
					ushort num4 = (ushort)((binBytes[num3] << 8) | binBytes[num3 + 1]);
					dataGridView1[k, 0].Value = ((double)(int)num4 / 10.0).ToString("F1");
					dataGridView1[k, 0].Style.BackColor = Color.LightGray;
					dataGridView1[k, 0].Style.ForeColor = Color.Black;
					dataGridView1[k, 0].ReadOnly = true;
				}
				else
				{
					dataGridView1[k, 0].Value = "N/A";
					dataGridView1[k, 0].Style.BackColor = Color.LightGray;
					dataGridView1[k, 0].Style.ForeColor = Color.Black;
					dataGridView1[k, 0].ReadOnly = true;
				}
			}
			for (int l = 1; l < num; l++)
			{
				if (l >= dataGridView1.RowCount)
				{
					dataGridView1.Rows.Add();
				}
				if (dataGridView1.ColumnCount > 0)
				{
					int num5 = rpmh + (l - 1) * 2;
					if (num5 + 1 < binBytes.Length)
					{
						ushort num6 = (ushort)((binBytes[num5] << 8) | binBytes[num5 + 1]);
						dataGridView1[0, l].Value = ((float)(int)num6 / kon).ToString("F0");
						dataGridView1[0, l].Style.BackColor = Color.LightGray;
						dataGridView1[0, l].Style.ForeColor = Color.Black;
						dataGridView1[0, l].ReadOnly = true;
					}
					else
					{
						dataGridView1[0, l].Value = "N/A";
						dataGridView1[0, l].Style.BackColor = Color.LightGray;
						dataGridView1[0, l].Style.ForeColor = Color.Black;
						dataGridView1[0, l].ReadOnly = true;
					}
				}
			}
			minValue = ushort.MaxValue;
			maxValue = 0;
			for (int m = 1; m < num; m++)
			{
				if (m >= dataGridView1.RowCount)
				{
					continue;
				}
				for (int n = 1; n < num2 + 1; n++)
				{
					if (n < dataGridView1.ColumnCount)
					{
						int num7 = selectedStartAddress + ((m - 1) * num2 + (n - 1)) * 2;
						if (num7 + 1 < binBytes.Length && num7 + 1 < binBytesORI.Length)
						{
							ushort num8 = (ushort)((binBytes[num7] << 8) | binBytes[num7 + 1]);
							ushort num9 = (ushort)((binBytesORI[num7] << 8) | binBytesORI[num7 + 1]);
							if (isCompared)
							{
								int num10 = num8 - num9;
								string arg;
								double num11;
								if (num8 >= num9)
								{
									arg = "+";
									num11 = ConvertUShortToValue((ushort)num10);
								}
								else
								{
									arg = "-";
									num11 = ConvertUShortToValue((ushort)(num9 - num8));
								}
								string value = $"({arg}{num11})";
								dataGridView1[n, m].Value = value;
							}
							else
							{
								if (num8 < minValue)
								{
									minValue = num8;
								}
								if (num8 > maxValue)
								{
									maxValue = num8;
								}
								double num12 = ConvertUShortToValue2(num8);
								dataGridView1[n, m].Value = num12.ToString("F1");
							}
						}
						else
						{
							dataGridView1[n, m].Value = "N/A";
						}
					}
					else
					{
						dataGridView1[n, m].Value = "N/A";
					}
				}
			}
		}
		else if (selectedStartAddress == 210634 || selectedStartAddress == 219134)
		{
			for (int num13 = 1; num13 < num2 + 1; num13++)
			{
				int num14 = tpsh + (num13 - 1) * 2;
				if (num14 + 1 < binBytes.Length)
				{
					ushort num15 = BitConverter.ToUInt16(new byte[2]
					{
						binBytes[num14],
						binBytes[num14 + 1]
					}, 0);
					dataGridView1[num13, 0].Value = ((double)(int)num15 / 10.0).ToString("F1");
					dataGridView1[num13, 0].Style.BackColor = Color.LightGray;
					dataGridView1[num13, 0].Style.ForeColor = Color.Black;
					dataGridView1[num13, 0].ReadOnly = true;
				}
				else
				{
					dataGridView1[num13, 0].Value = "N/A";
					dataGridView1[num13, 0].Style.BackColor = Color.LightGray;
					dataGridView1[num13, 0].Style.ForeColor = Color.Black;
					dataGridView1[num13, 0].ReadOnly = true;
				}
			}
			for (int num16 = 1; num16 < num; num16++)
			{
				if (num16 >= dataGridView1.RowCount)
				{
					dataGridView1.Rows.Add();
				}
				if (dataGridView1.ColumnCount > 0)
				{
					int num17 = rpmh + (num16 - 1) * 2;
					if (num17 + 1 < binBytes.Length)
					{
						ushort num18 = BitConverter.ToUInt16(new byte[2]
						{
							binBytes[num17],
							binBytes[num17 + 1]
						}, 0);
						dataGridView1[0, num16].Value = ((float)(int)num18 / kon).ToString("F0");
						dataGridView1[0, num16].Style.BackColor = Color.LightGray;
						dataGridView1[0, num16].Style.ForeColor = Color.Black;
						dataGridView1[0, num16].ReadOnly = true;
					}
					else
					{
						dataGridView1[0, num16].Value = "N/A";
						dataGridView1[0, num16].Style.BackColor = Color.LightGray;
						dataGridView1[0, num16].Style.ForeColor = Color.Black;
						dataGridView1[0, num16].ReadOnly = true;
					}
				}
			}
			minValue = ushort.MaxValue;
			maxValue = 0;
			for (int num19 = 1; num19 < num; num19++)
			{
				if (num19 >= dataGridView1.RowCount)
				{
					continue;
				}
				for (int num20 = 1; num20 < num2 + 1; num20++)
				{
					if (num20 < dataGridView1.ColumnCount)
					{
						int num21 = selectedStartAddress + ((num19 - 1) * num2 + (num20 - 1)) * 2;
						if (num21 + 1 < binBytes.Length && num21 + 1 < binBytesORI.Length)
						{
							ushort num22 = BitConverter.ToUInt16(binBytes, num21);
							ushort num23 = BitConverter.ToUInt16(binBytesORI, num21);
							if (isCompared)
							{
								double val = ((num22 < num23) ? ((double)(int)num23 - (double)(int)num22) : ((double)(int)num22 - (double)(int)num23));
								ushort difference = (ushort)Math.Max(0.0, Math.Min(65535.0, val));
								double num24 = ConvertUShortToValue3(difference);
								string arg2 = ((num22 >= num23) ? "+" : "-");
								string value2 = FormatCompareText(ConvertUShortToValue3(num22), ConvertUShortToValue3(num23), "+0.0;-0.0;0.0");
								dataGridView1[num20, num19].Value = value2;
								continue;
							}
							if (num22 < minValue)
							{
								minValue = num22;
							}
							if (num22 > maxValue)
							{
								maxValue = num22;
							}
							double num25 = ConvertUShortToValue4(num22);
							dataGridView1[num20, num19].Value = num25.ToString("F1");
						}
						else
						{
							dataGridView1[num20, num19].Value = "N/A";
						}
					}
					else
					{
						dataGridView1[num20, num19].Value = "N/A";
					}
				}
			}
		}
		else
		{
			for (int num26 = 1; num26 < num2 + 1; num26++)
			{
				int num27 = tpsh + (num26 - 1) * 2;
				if (num27 + 1 < binBytes.Length)
				{
					ushort num28 = BitConverter.ToUInt16(new byte[2]
					{
						binBytes[num27],
						binBytes[num27 + 1]
					}, 0);
					dataGridView1[num26, 0].Value = ((double)(int)num28 / 10.0).ToString("F1");
					dataGridView1[num26, 0].Style.BackColor = Color.LightGray;
					dataGridView1[num26, 0].Style.ForeColor = Color.Black;
					dataGridView1[num26, 0].ReadOnly = true;
				}
				else
				{
					dataGridView1[num26, 0].Value = "N/A";
					dataGridView1[num26, 0].Style.BackColor = Color.LightGray;
					dataGridView1[num26, 0].Style.ForeColor = Color.Black;
					dataGridView1[num26, 0].ReadOnly = true;
				}
			}
			for (int num29 = 1; num29 < num; num29++)
			{
				if (num29 >= dataGridView1.RowCount)
				{
					dataGridView1.Rows.Add();
				}
				if (dataGridView1.ColumnCount > 0)
				{
					int num30 = rpmh + (num29 - 1) * 2;
					if (num30 + 1 < binBytes.Length)
					{
						ushort num31 = BitConverter.ToUInt16(new byte[2]
						{
							binBytes[num30],
							binBytes[num30 + 1]
						}, 0);
						dataGridView1[0, num29].Value = ((float)(int)num31 / kon).ToString("F0");
						dataGridView1[0, num29].Style.BackColor = Color.LightGray;
						dataGridView1[0, num29].Style.ForeColor = Color.Black;
						dataGridView1[0, num29].ReadOnly = true;
					}
					else
					{
						dataGridView1[0, num29].Value = "N/A";
						dataGridView1[0, num29].Style.BackColor = Color.LightGray;
						dataGridView1[0, num29].Style.ForeColor = Color.Black;
						dataGridView1[0, num29].ReadOnly = true;
					}
				}
			}
			minValue = 255;
			maxValue = 0;
			for (int num32 = 1; num32 < num; num32++)
			{
				if (num32 >= dataGridView1.RowCount)
				{
					continue;
				}
				for (int num33 = 1; num33 < num2 + 1; num33++)
				{
					if (num33 < dataGridView1.ColumnCount)
					{
						int num34 = selectedStartAddress + ((num32 - 1) * num2 + (num33 - 1));
						if (num34 + 1 < binBytes.Length && num34 + 1 < binBytesORI.Length)
						{
							byte b = binBytes[num34];
							byte b2 = binBytesORI[num34];
							if (isCompared)
							{
								double val2 = ((b < b2) ? ((double)(int)b2 - (double)(int)b) : ((double)(int)b - (double)(int)b2));
								byte byteValue = (byte)Math.Max(0.0, Math.Min(255.0, val2));
								double num35 = ConvertByteToValue2(byteValue);
								string arg3 = ((b >= b2) ? "+" : "-");
								string value3 = $"({arg3}{num35})";
								dataGridView1[num33, num32].Value = value3;
								continue;
							}
							if (b < minValue)
							{
								minValue = b;
							}
							if (b > maxValue)
							{
								maxValue = b;
							}
							double num36 = ConvertByteToValue(b);
							Debug.WriteLine($"Assigning to DataGridView[{num33}, {num32}] = {num36}");
							dataGridView1[num33, num32].Value = num36.ToString("F1");
						}
						else
						{
							dataGridView1[num33, num32].Value = "N/A";
						}
					}
					else
					{
						dataGridView1[num33, num32].Value = "N/A";
					}
				}
			}
		}
		ukuran();
		static double ConvertUShortToValue(ushort num37)
		{
			return (double)(int)num37 / 255.0 * 19.827 - 11.53 + 11.53;
		}
		static double ConvertUShortToValue2(ushort num37)
		{
			return (double)(int)num37 / 255.0 * 19.827 - 11.53;
		}
		static double ConvertUShortToValue3(ushort num37)
		{
			return (double)(int)num37 / 255.0 * 19.827 - 11.53 + 11.53;
		}
		static double ConvertUShortToValue4(ushort num37)
		{
			return (double)(int)num37 / 255.0 * 19.827 - 11.53;
		}
	}

	private void TPSBREAK()
	{
		if (binBytes == null || binBytes.Length == 0)
		{
			return;
		}
		dataGridView1.Rows.Clear();
		dataGridView1.Columns.Clear();
		dataGridView1.Invalidate();
		int num = 65535;
		int num2 = 0;
		int num3 = 1;
		if (dataGridView1.ColumnCount < columnCount)
		{
			dataGridView1.ColumnCount = columnCount + 1;
		}
		if (dataGridView1.RowCount < num3 + 1)
		{
			dataGridView1.RowCount = num3 + 1;
		}
		for (int i = 1; i <= columnCount; i++)
		{
			dataGridView1[i, 0].Value = i;
			dataGridView1[i, 0].ReadOnly = true;
			dataGridView1[i, 0].Style.BackColor = Color.LightGray;
		}
		dataGridView1[0, num3].Value = "1";
		dataGridView1[0, num3].ReadOnly = true;
		dataGridView1[0, num3].Style.BackColor = Color.LightGray;
		foreach (DataGridViewColumn column in dataGridView1.Columns)
		{
			column.SortMode = DataGridViewColumnSortMode.NotSortable;
		}
		if (selectedStartAddress == 238056 || selectedStartAddress == 233644)
		{
			if (num3 < dataGridView1.RowCount)
			{
				for (int j = 1; j <= columnCount; j++)
				{
					if (j < dataGridView1.ColumnCount)
					{
						int num4 = selectedStartAddress + (num3 - 1) * columnCount * 2 + (j - 1) * 2;
						if (num4 + 1 < binBytes.Length)
						{
							ushort num5 = (ushort)((binBytes[num4] << 8) | binBytes[num4 + 1]);
							if (num5 < num)
							{
								num = num5;
							}
							if (num5 > num2)
							{
								num2 = num5;
							}
							dataGridView1[j, num3].Value = ((double)(int)num5 / 10.0).ToString("F1");
						}
						else
						{
							dataGridView1[j, num3].Value = "N/A";
						}
					}
					else
					{
						dataGridView1[j, num3].Value = "N/A";
					}
				}
			}
		}
		else if (num3 < dataGridView1.RowCount)
		{
			for (int k = 1; k <= columnCount; k++)
			{
				if (k < dataGridView1.ColumnCount)
				{
					int num6 = selectedStartAddress + (num3 - 1) * columnCount * 2 + (k - 1) * 2;
					if (num6 + 1 < binBytes.Length)
					{
						ushort num7 = BitConverter.ToUInt16(new byte[2]
						{
							binBytes[num6],
							binBytes[num6 + 1]
						}, 0);
						if (num7 < num)
						{
							num = num7;
						}
						if (num7 > num2)
						{
							num2 = num7;
						}
						dataGridView1[k, num3].Value = ((double)(int)num7 / 10.0).ToString("F1");
					}
					else
					{
						dataGridView1[k, num3].Value = "N/A";
					}
				}
				else
				{
					dataGridView1[k, num3].Value = "N/A";
				}
			}
		}
		ukuran3();
	}

	private void RPMBREAKPOIN()
	{
		if (binBytes == null || binBytes.Length == 0)
		{
			return;
		}
		dataGridView1.Rows.Clear();
		dataGridView1.Columns.Clear();
		dataGridView1.Invalidate();
		int num = 2;
		int num2 = rowCount + 1;
		dataGridView1.ColumnCount = num;
		dataGridView1.RowCount = num2;
		for (int i = 1; i < num2; i++)
		{
			dataGridView1[0, i].Value = i.ToString();
			dataGridView1[0, i].Style.BackColor = Color.LightGray;
			dataGridView1[0, i].Style.ForeColor = Color.Black;
			dataGridView1[0, i].ReadOnly = true;
		}
		double num3 = double.MaxValue;
		double num4 = double.MinValue;
		for (int j = 1; j < num2; j++)
		{
			int num5 = selectedStartAddress + (j - 1) * 2;
			if (num5 + 1 < binBytes.Length)
			{
				ushort num6 = BitConverter.ToUInt16(new byte[2]
				{
					binBytes[num5],
					binBytes[num5 + 1]
				}, 0);
				double num7 = (int)num6;
				if (num7 < num3)
				{
					num3 = num7;
				}
				if (num7 > num4)
				{
					num4 = num7;
				}
			}
		}
		if (num3 == num4)
		{
			num3 = 0.0;
		}
		if (selectedStartAddress == 238108 || selectedStartAddress == 233696)
		{
			for (int k = 1; k < num2; k++)
			{
				int num8 = selectedStartAddress + (k - 1) * 2;
				if (num8 + 1 < binBytes.Length)
				{
					ushort num9 = (ushort)((binBytes[num8] << 8) | binBytes[num8 + 1]);
					double num10 = (float)(int)num9 / kon;
					if (num10 < num3)
					{
						num3 = num10;
					}
					if (num10 > num4)
					{
						num4 = num10;
					}
					dataGridView1[1, k].Value = num10.ToString("F0");
					dataGridView1[1, k].Style.BackColor = Color.LightGray;
					dataGridView1[1, k].Style.ForeColor = Color.Black;
				}
				else
				{
					dataGridView1[1, k].Value = "N/A";
					dataGridView1[1, k].Style.BackColor = Color.LightGray;
					dataGridView1[1, k].Style.ForeColor = Color.Black;
				}
			}
		}
		else
		{
			for (int l = 1; l < num2; l++)
			{
				int num11 = selectedStartAddress + (l - 1) * 2;
				if (num11 + 1 < binBytes.Length)
				{
					ushort num12 = BitConverter.ToUInt16(new byte[2]
					{
						binBytes[num11],
						binBytes[num11 + 1]
					}, 0);
					double num13 = (float)(int)num12 / kon;
					if (num13 < num3)
					{
						num3 = num13;
					}
					if (num13 > num4)
					{
						num4 = num13;
					}
					dataGridView1[1, l].Value = num13.ToString("F0");
					dataGridView1[1, l].Style.BackColor = Color.LightGray;
					dataGridView1[1, l].Style.ForeColor = Color.Black;
				}
				else
				{
					dataGridView1[1, l].Value = "N/A";
					dataGridView1[1, l].Style.BackColor = Color.LightGray;
					dataGridView1[1, l].Style.ForeColor = Color.Black;
				}
			}
		}
		ukuran2();
	}

	private void IAPBREAK()
	{
		if (binBytes == null || binBytes.Length == 0)
		{
			return;
		}
		dataGridView1.Rows.Clear();
		dataGridView1.Columns.Clear();
		int num = 255;
		int num2 = 0;
		int num3 = 1;
		if (dataGridView1.ColumnCount < columnCount)
		{
			dataGridView1.ColumnCount = columnCount + 1;
		}
		if (dataGridView1.RowCount < num3 + 1)
		{
			dataGridView1.RowCount = num3 + 1;
		}
		for (int i = 1; i <= columnCount; i++)
		{
			dataGridView1[i, 0].Value = i;
			dataGridView1[i, 0].ReadOnly = true;
			dataGridView1[i, 0].Style.BackColor = Color.LightGray;
		}
		foreach (DataGridViewColumn column in dataGridView1.Columns)
		{
			column.SortMode = DataGridViewColumnSortMode.NotSortable;
		}
		if (num3 < dataGridView1.RowCount)
		{
			for (int j = 1; j <= columnCount; j++)
			{
				if (j < dataGridView1.ColumnCount)
				{
					int num4 = selectedStartAddress + (num3 - 1) * columnCount + (j - 1);
					if (num4 < binBytes.Length)
					{
						byte b = binBytes[num4];
						if (b < num)
						{
							num = b;
						}
						if (b > num2)
						{
							num2 = b;
						}
						dataGridView1[j, num3].Value = ((double)(int)b / 2.514).ToString("F1");
					}
					else
					{
						dataGridView1[j, num3].Value = "N/A";
					}
				}
				else
				{
					dataGridView1[j, num3].Value = "N/A";
				}
			}
		}
		ukuran3();
	}

	private void ukuran3()
	{
		if (dataGridView1.Rows.Count <= 0)
		{
			return;
		}
		ukur = true;
		dataGridView1.Dock = DockStyle.None;
		dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		int num = 50;
		dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
		foreach (DataGridViewColumn column in dataGridView1.Columns)
		{
			column.Width = num;
			column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
		}
		int num2 = 25;
		foreach (DataGridViewRow item in (IEnumerable)dataGridView1.Rows)
		{
			item.Height = num2;
		}
		dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
		dataGridView1.ColumnHeadersHeight = 20;
		float emSize = 8f;
		Font font = new Font("Arial", emSize, FontStyle.Regular);
		dataGridView1.DefaultCellStyle.Font = font;
		dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
		dataGridView1.ColumnHeadersDefaultCellStyle.Font = font;
		dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
		dataGridView1.Width = panelDATA.ClientSize.Width;
		dataGridView1.Height = panelDATA.ClientSize.Height;
		dataGridView1.Refresh();
	}

	private void limiter1()
	{
		if (binBytes == null || binBytes.Length == 0)
		{
			return;
		}
		dataGridView1.Rows.Clear();
		dataGridView1.Columns.Clear();
		int num = 2;
		int num2 = 3;
		int num3 = 65535;
		int num4 = 0;
		dataGridView1.ColumnCount = num2;
		dataGridView1.RowCount = num;
		for (int i = 1; i < num2; i++)
		{
			dataGridView1[i, 0].Value = i;
			dataGridView1[i, 0].ReadOnly = true;
			dataGridView1[i, 0].Style.BackColor = Color.LightGray;
		}
		foreach (DataGridViewColumn column in dataGridView1.Columns)
		{
			column.SortMode = DataGridViewColumnSortMode.NotSortable;
		}
		int num5 = 2;
		int num6 = 2;
		switch (selectedStartAddress)
		{
		case 105744:
		case 105840:
		case 105844:
		case 106028:
		case 234008:
		case 239940:
		case 240602:
		case 245100:
			num6 = 4;
			break;
		case 3182:
		case 3456:
		case 3588:
		case 5275:
		case 7342:
		case 8215:
		case 8433:
		case 9260:
		case 9493:
		case 10320:
		case 15959:
		case 16048:
		case 16072:
		case 16074:
		case 20282:
		case 20290:
		case 20382:
		case 21032:
		case 22286:
		case 38530:
		case 38627:
		case 38628:
		case 38646:
		case 38734:
		case 38758:
		case 38787:
		case 38788:
		case 38824:
		case 38878:
		case 44179:
		case 44295:
		case 44330:
		case 44386:
		case 44395:
		case 44404:
		case 44541:
		case 45006:
		case 45122:
		case 45163:
		case 45219:
		case 45228:
		case 45237:
		case 45374:
		case 46523:
		case 46744:
		case 46933:
		case 46978:
		case 47015:
		case 47042:
		case 47105:
		case 47429:
		case 47466:
		case 47493:
		case 47556:
			num6 = 10;
			break;
		case 43901:
			num6 = 40;
			break;
		case 47036:
		case 47100:
			num6 = 16;
			break;
		case 13436:
			num6 = 18;
			break;
		}
		Console.WriteLine($"Selected Address: 0x{selectedStartAddress:X}, Stride: {num5}, MinorStride: {num6}");
		if (selectedStartAddress == 235344 || selectedStartAddress == 235684 || selectedStartAddress == 235836 || selectedStartAddress == 236008 || selectedStartAddress == 230932 || selectedStartAddress == 231272 || selectedStartAddress == 231424 || selectedStartAddress == 231596 || selectedStartAddress == 237002 || selectedStartAddress == 241414)
		{
			for (int j = 1; j < num; j++)
			{
				for (int k = 1; k < num2; k++)
				{
					int num7 = selectedStartAddress + (j - 1) * num2 * num5 + (k - 1) * num6;
					if (num7 + 1 < binBytes.Length)
					{
						ushort num8 = (ushort)((binBytes[num7] << 8) | binBytes[num7 + 1]);
						if (num8 < num3)
						{
							num3 = num8;
						}
						if (num8 > num4)
						{
							num4 = num8;
						}
						dataGridView1[k, j].Value = ((float)(int)num8 / lm).ToString("F0");
					}
					else
					{
						dataGridView1[k, j].Value = "N/A";
					}
				}
			}
		}
		else
		{
			for (int l = 1; l < num; l++)
			{
				for (int m = 1; m < num2; m++)
				{
					int num9 = selectedStartAddress + (l - 1) * num2 * num5 + (m - 1) * num6;
					if (num9 + 1 < binBytes.Length)
					{
						ushort num10 = BitConverter.ToUInt16(new byte[2]
						{
							binBytes[num9],
							binBytes[num9 + 1]
						}, 0);
						if (num10 < num3)
						{
							num3 = num10;
						}
						if (num10 > num4)
						{
							num4 = num10;
						}
						dataGridView1[m, l].Value = ((float)(int)num10 / lm).ToString("F0");
					}
					else
					{
						dataGridView1[m, l].Value = "N/A";
					}
				}
			}
		}
		ukuran2();
	}

	private void limiter5()
	{
		if (binBytes == null || binBytes.Length == 0)
		{
			return;
		}
		dataGridView1.Rows.Clear();
		dataGridView1.Columns.Clear();
		int num = 2;
		int num2 = 3;
		int num3 = 65535;
		int num4 = 0;
		dataGridView1.ColumnCount = num2;
		dataGridView1.RowCount = num;
		for (int i = 1; i < num2; i++)
		{
			dataGridView1[i, 0].Value = i;
			dataGridView1[i, 0].ReadOnly = true;
			dataGridView1[i, 0].Style.BackColor = Color.LightGray;
		}
		foreach (DataGridViewColumn column in dataGridView1.Columns)
		{
			column.SortMode = DataGridViewColumnSortMode.NotSortable;
		}
		for (int j = 1; j < num; j++)
		{
			for (int k = 1; k < num2; k++)
			{
				int num5 = selectedStartAddress + (j - 1) * num2 * 2 + (k - 1) * 2;
				if (num5 + 1 < binBytes.Length)
				{
					ushort num6 = BitConverter.ToUInt16(new byte[2]
					{
						binBytes[num5],
						binBytes[num5 + 1]
					}, 0);
					if (num6 < num3)
					{
						num3 = num6;
					}
					if (num6 > num4)
					{
						num4 = num6;
					}
					dataGridView1[k, j].Value = ((double)(int)num6 / 1.0).ToString("F0");
				}
				else
				{
					dataGridView1[k, j].Value = "N/A";
				}
			}
		}
		ukuran2();
	}

	private void O2(int alamat, byte mask)
	{
		if (binBytes == null || binBytes.Length == 0)
		{
			return;
		}
		activeAddress = alamat;
		activeMask = mask;
		dataGridView1.Rows.Clear();
		dataGridView1.Columns.Clear();
		int num = 1;
		int num2 = 2;
		for (int i = 1; i < num2; i++)
		{
			DataGridViewButtonColumn dataGridViewColumn = new DataGridViewButtonColumn
			{
				HeaderText = $"Switch {i}",
				Name = $"Switch{i}",
				AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
				Text = "NonAktif",
				UseColumnTextForButtonValue = false
			};
			dataGridView1.Columns.Add(dataGridViewColumn);
		}
		dataGridView1.RowCount = num;
		if (activeAddress < binBytes.Length)
		{
			byte b = binBytes[activeAddress];
			for (int j = 1; j < num2; j++)
			{
				bool flag = (b & activeMask) == activeMask;
				dataGridView1[j - 1, 0].Value = (flag ? "Aktif" : "NonAktif");
			}
		}
		else
		{
			for (int k = 1; k < num2; k++)
			{
				dataGridView1[k - 1, 0].Value = "NonAktif";
			}
		}
		ukuran2();
	}

	private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
	{
		if (e.RowIndex < 0 || e.ColumnIndex < 0 || !(dataGridView1.Columns[e.ColumnIndex] is DataGridViewButtonColumn))
		{
			return;
		}
		string text = dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString();
		if (text == "NonAktif")
		{
			dataGridView1[e.ColumnIndex, e.RowIndex].Value = "Aktif";
		}
		else
		{
			dataGridView1[e.ColumnIndex, e.RowIndex].Value = "NonAktif";
		}
		if (activeAddress >= 0 && activeAddress < binBytes.Length)
		{
			if (dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString() == "Aktif")
			{
				binBytes[activeAddress] |= activeMask;
			}
			else
			{
				binBytes[activeAddress] &= (byte)(~activeMask);
			}
		}
		else
		{
			Console.WriteLine($"Error: Active address or mask is invalid. Address: {activeAddress}, Mask: {activeMask}");
		}
	}

	private void SaveState()
	{
		if (binBytes != null)
		{
			if (undoStack.Count == 0 || !SameBytes(undoStack.Peek(), binBytes))
			{
				undoStack.Push((byte[])binBytes.Clone());
				redoStack.Clear();
			}
		}
	}

	private static bool SameBytes(byte[] a, byte[] b)
	{
		if (a == null || b == null || a.Length != b.Length)
		{
			return false;
		}
		for (int i = 0; i < a.Length; i++)
		{
			if (a[i] != b[i])
			{
				return false;
			}
		}
		return true;
	}

	private void Undo()
	{
		if (binBytes == null)
		{
			return;
		}
		RunWithRealValues(() => SaveEditedValues2());
		if (undoStack.Count == 0 || !SameBytes(undoStack.Peek(), binBytes))
		{
			undoStack.Push((byte[])binBytes.Clone());
		}
		if (undoStack.Count < 2)
		{
			return;
		}
		redoStack.Push(undoStack.Pop());
		binBytes = (byte[])undoStack.Peek().Clone();
		RefreshAfterHistory();
	}

	private void Redo()
	{
		if (binBytes == null || redoStack.Count == 0)
		{
			return;
		}
		byte[] next = redoStack.Pop();
		undoStack.Push((byte[])next.Clone());
		binBytes = (byte[])next.Clone();
		RefreshAfterHistory();
	}

	private void RefreshAfterHistory()
	{
		if (selectedStartAddress == 0)
		{
			DisplayData();
		}
		else
		{
			RefreshCurrentTable();
		}
		RefreshOpenGraph();
	}

	private byte[] EncryptBytes(byte[] plainBytes)
	{
		using Aes aes = Aes.Create();
		aes.Key = Key;
		aes.IV = IV;
		ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);
		using MemoryStream memoryStream = new MemoryStream();
		using CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
		cryptoStream.Write(plainBytes, 0, plainBytes.Length);
		cryptoStream.FlushFinalBlock();
		return memoryStream.ToArray();
	}

	private void breakpoinRPMToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			button9.Enabled = true;
			selectedStartAddress = rpmh;
			RPMBREAKPOIN();
		}
	}

	private void breakpointIAPToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			button9.Enabled = true;
			selectedStartAddress = iaph;
			IAPBREAK();
		}
	}

	private void breakpointTPSToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			button9.Enabled = true;
			selectedStartAddress = tpsh;
			TPSBREAK();
		}
	}

	private void rpmVsTps1ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			button9.Enabled = true;
			selectedStartAddress = feul1;
			TPS2Values();
		}
	}

	private void rpmVsTps2ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			button9.Enabled = true;
			selectedStartAddress = feul2;
			TPS3Values();
		}
	}

	private void toolStripMenuItem2_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			button9.Enabled = true;
			MasukkanNilaiBaru(sender, e);
			SaveEditedValues2();
			SaveState();
		}
	}

	private void toolStripMenuItem3_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			button9.Enabled = true;
			KurangiNilai(sender, e);
			SaveEditedValues2();
			SaveState();
		}
	}

	private void toolStripMenuItem4_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			TambahNilai(sender, e);
			SaveEditedValues2();
			SaveState();
		}
	}

	private void kaliNilaiToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			KaliNilai(sender, e);
			SaveEditedValues2();
			SaveState();
		}
	}

	private void bagiNilaiToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			BagiNilai(sender, e);
			SaveEditedValues2();
			SaveState();
		}
	}

	private void smoothToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			TambahNilaiInterpolasi(sender, e);
			SaveEditedValues2();
			SaveState();
		}
	}

	private void button9_Click(object sender, EventArgs e)
	{
		QuickSave();
	}

	private void QuickSave()
	{
		if (binBytes == null)
		{
			MessageBox.Show("Belum ada bin yang dibuka.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
		RunWithRealValues(() =>
		{
			SaveEditedValues();
			SaveState();
		});
		string stamp = DateTime.Now.ToString("HH:mm:ss");
		string title = (baseTitle.Length > 0) ? baseTitle : Text;
		Text = title + "  |  Edit tersimpan " + stamp;
		System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
		timer.Interval = 2500;
		timer.Tick += delegate
		{
			timer.Stop();
			timer.Dispose();
			Text = title;
		};
		timer.Start();
	}

	public void SaveBinBytesWithDialog(byte[] binBytes)
	{
		byte[] bytes = EncryptBytes(binBytes);
		using SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "Files (*.acg)|*.acg|All Files (*.*)|*.*";
		saveFileDialog.Title = "Save .acg File";
		saveFileDialog.DefaultExt = ".acg";
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			string fileName = saveFileDialog.FileName;
			try
			{
				File.WriteAllBytes(fileName, bytes);
				lastSavePath = fileName;
				MessageBox.Show("Data berhasil disimpan ke " + fileName, "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Terjadi kesalahan saat menyimpan file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
		}
	}

	private void button11_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			SaveBinBytesWithDialog(binBytes);
		}
	}

	private void toolStripMenuItem6_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			button9.Enabled = true;
			selectedStartAddress = ig1;
			IGNITION1();
		}
	}

	private void toolStripMenuItem10_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			button9.Enabled = true;
			selectedStartAddress = limit1;
			limiter1();
		}
	}

	private void toolStripMenuItem11_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			button9.Enabled = true;
			selectedStartAddress = limit2;
			limiter1();
		}
	}

	private void toolStripMenuItem12_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			button9.Enabled = true;
			selectedStartAddress = limit3;
			limiter1();
		}
	}

	private void toolStripMenuItem13_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			button9.Enabled = true;
			selectedStartAddress = limit4;
			limiter1();
		}
	}

	private void toolStripMenuItem14_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			button9.Enabled = true;
			selectedStartAddress = limit5;
			limiter1();
		}
	}

	private void AddClickHandlers(ToolStripItemCollection items)
	{
		foreach (ToolStripItem item in items)
		{
			if (item is ToolStripMenuItem toolStripMenuItem)
			{
				toolStripMenuItem.Click += MenuItem_Click;
				if (toolStripMenuItem.DropDownItems.Count > 0)
				{
					AddClickHandlers(toolStripMenuItem.DropDownItems);
				}
			}
		}
	}

	private void MenuItem_Click(object sender, EventArgs e)
	{
		ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
		if (toolStripMenuItem == null)
		{
		}
	}

	private void toolStripMenuItem17_Click(object sender, EventArgs e)
	{
		currentMenu = "O2";
		O2(o2, o2mask);
	}

	private void toolStripMenuItem18_Click(object sender, EventArgs e)
	{
		currentMenu = "O2C";
		O2(o2c, o2cmask);
	}

	private void toolStripMenuItem16_Click(object sender, EventArgs e)
	{
		currentMenu = "IMMO";
		O2(IMMO, immomask);
	}

	private void toolStripMenuItem19_Click(object sender, EventArgs e)
	{
		currentMenu = "MODE 0";
		O2(MODE0, immomask);
	}

	private void limiter6ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			selectedStartAddress = limit6;
			limiter1();
		}
	}

	private void toolStripMenuItem20_Click(object sender, EventArgs e)
	{
		currentMenu = "MODE 1";
		O2(MODE1, immomask);
	}

	private void toolStripMenuItem21_Click(object sender, EventArgs e)
	{
		currentMenu = "MODE 2";
		O2(MODE2, immomask);
	}

	private void toolStripMenuItem22_Click(object sender, EventArgs e)
	{
		currentMenu = "MODE 3";
		O2(MODE3, immomask);
	}

	private void offMode4ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		currentMenu = "MODE 4";
		O2(MODE4, immomask);
	}

	private void toolStripMenuItem7_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			selectedStartAddress = ig2;
			IGNITION1();
		}
	}

	private void rpmVsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			selectedStartAddress = feul3;
			TPS2Values();
		}
	}

	private void rpmVsIAPToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			selectedStartAddress = iap;
			IAPValues();
		}
	}

	private void toolStripMenuItem8_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			selectedStartAddress = ig3;
			IGNITION1();
		}
	}

	private void compareToDataBaseToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			isCompared = !isCompared;
			if (selectedStartAddress == feul1 || selectedStartAddress == feul2 || selectedStartAddress == feul3)
			{
				TPS2Values();
			}
			if (selectedStartAddress == ig1 || selectedStartAddress == ig2 || selectedStartAddress == ig3 || selectedStartAddress == ig4)
			{
				IGNITION1();
			}
			if (selectedStartAddress == iap)
			{
				IAPValues();
			}
		}
	}

	private void BtnWrite_Click_1(object sender, EventArgs e)
	{
		string text = GetProcessorId().Trim();
		string text2 = GetDiskSerialNumber().Trim();
		string serialNumber = text + "_" + text2;
		string text3 = ProcessDiskSerial(serialNumber);
		string[] array = DecryptKeyFromFile();
		string text4 = array[0];
		string value = array[1];
		string value2 = array[2];
		if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(value2) && !(text3 != text4))
		{
			if (!ConfirmBinMatchesSelection(binBytes, "FLASH ke ECU"))
			{
				return;
			}
			Thread thread = new Thread(flas)
			{
				IsBackground = true
			};
			thread.Start();
			button1.Enabled = true;
		}
	}

	private void InitializeLabelMovement()
	{
		labelMovementTimer = new System.Windows.Forms.Timer();
		labelMovementTimer.Interval = 100;
		labelMovementTimer.Tick += LabelMovementTimer_Tick;
		labelMovementTimer.Start();
	}

	private void LabelMovementTimer_Tick(object sender, EventArgs e)
	{
		labelOffset -= 5;
		if (labelOffset < -TextRenderer.MeasureText(label14.Text, label14.Font).Width)
		{
			labelOffset = label14.Width;
		}
		label14.Padding = new Padding(labelOffset, 0, 0, 0);
	}

	private void button3_Click(object sender, EventArgs e)
	{
		string text = GetProcessorId().Trim();
		string text2 = GetDiskSerialNumber().Trim();
		string text3 = text + "_" + text2;
		string text4 = ProcessDiskSerial(text3);
		string[] array = DecryptKeyFromFile();
		string text5 = array[0];
		string value = array[1];
		string value2 = array[2];
		if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(value2) || text4 != text5)
		{
			ShowSaveFileDialog(text3);
			MessageBox.Show("Invalid serial number or missing user information. The application will now exit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			Application.Exit();
			button3.Enabled = false;
		}
		else
		{
			con = true;
			endThread = true;
			Thread thread = new Thread(WorkerThread)
			{
				IsBackground = true
			};
			thread.Start();
		}
	}

	private void button1_Click(object sender, EventArgs e)
	{
		if (binBytes == null)
		{
			return;
		}
		ukuranpointer = 12;
		StoreInitialCellValues();
		if (!isTaskRunning)
		{
			showPointer = !showPointer;
			dataGridView1.Invalidate();
			_cancellationTokenSource = new CancellationTokenSource();
			InitFtdiIso();
			Thread thread = new Thread(() =>
			{
				LoopDat17(_cancellationTokenSource.Token);
			})
			{
				IsBackground = true
			};
			thread.Start();
			updateTimer = new System.Windows.Forms.Timer();
			updateTimer.Interval = 100;
			updateTimer.Tick += (object s, EventArgs args) =>
			{
				if (chartForm != null)
				{
					chartForm.RPMD = RPMD;
					chartForm.TPSD = TPSD;
					chartForm.AD = AD;
					chartForm.AFR = AFR;
					chartForm.UpdateChart(RPMD, TPSD, AD, AFR);
				}
			};
			updateTimer.Start();
			isTaskRunning = true;
			button1.Text = "Stop";
		}
		else
		{
			showPointer = !showPointer;
			dataGridView1.Invalidate();
			_cancellationTokenSource.Cancel();
			isTaskRunning = false;
			if (updateTimer != null)
			{
				updateTimer.Stop();
				updateTimer.Dispose();
			}
			if (chartForm != null && !chartForm.IsDisposed)
			{
				chartForm.Close();
				chartForm.Dispose();
				chartForm = null;
			}
			button1.Text = "Live";
		}
	}

	private void dat10()
	{
		byte[] array = new byte[5] { 114, 5, 113, 16, 8 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		SendCommand(array, array.Length, ref reply, ref replySize);
		string text = BitConverter.ToString(reply, 0, (int)replySize);
		Console.WriteLine("Response dat10: " + text);
		if (reply[1] == 5)
		{
			tek10 = false;
			return;
		}
		try
		{
			RPMD = (reply[4] << 8) + reply[5];
			TPSD = (float)(int)reply[7] / 2f;
			float num = (float)(int)reply[8] * 5f / 256f;
			CL = reply[9] - 40;
			MAP = (int)reply[13];
			AD = (float)(int)reply[20] / 2f - 64f;
			inj = (float)((reply[18] << 8) + reply[19]) / 200f;
			IDC = (float)RPMD * inj / 1200f;
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error decoding data: " + ex.Message);
		}
		analogrpm();
		analogtps();
		dat20();
		hitunghp();
		hitungtq();
		if (liv)
		{
			FindAndPrintClosestValues2();
		}
		else
		{
			FindAndPrintClosestValues();
		}
	}

	private void dat11()
	{
		byte[] array = new byte[5] { 114, 5, 113, 17, 7 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		SendCommand(array, array.Length, ref reply, ref replySize);
		string text = BitConverter.ToString(reply, 0, (int)replySize);
		Console.WriteLine("Response 11: " + text);
		if (reply[1] == 5)
		{
			tek11 = false;
			return;
		}
		try
		{
			RPMD = (reply[4] << 8) + reply[5];
			TPSD = (float)(int)reply[7] / 2f;
			float num = (float)(int)reply[8] * 5f / 256f;
			CL = reply[9] - 40;
			MAP = (int)reply[13];
			AD = (float)(int)reply[20] / 2f - 64f;
			inj = (float)((reply[18] << 8) + reply[19]) / 200f;
			IDC = (float)RPMD * inj / 1200f;
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error decoding data: " + ex.Message);
		}
		analogrpm();
		analogtps();
		dat20();
		hitunghp();
		hitungtq();
		if (liv)
		{
			FindAndPrintClosestValues2();
		}
		else
		{
			FindAndPrintClosestValues();
		}
	}

	private void dat13()
	{
		byte[] array = new byte[5] { 114, 5, 113, 19, 5 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		SendCommand(array, array.Length, ref reply, ref replySize);
		string text = BitConverter.ToString(reply, 0, (int)replySize);
		Console.WriteLine("Response 13: " + text);
		if (reply[1] == 5)
		{
			tek13 = false;
			return;
		}
		try
		{
			RPMD = (reply[4] << 8) + reply[5];
			TPSD = (float)(int)reply[7] / 2f;
			float num = (float)(int)reply[8] * 5f / 256f;
			CL = reply[9] - 40;
			MAP = (int)reply[13];
			AD = (float)(int)reply[17] / 2f - 64f;
			inj = (float)((reply[15] << 8) + reply[16]) / 200f;
			IDC = (float)RPMD * inj / 1200f;
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error decoding data: " + ex.Message);
		}
		analogrpm();
		analogtps();
		dat20();
		hitunghp();
		hitungtq();
		if (liv)
		{
			FindAndPrintClosestValues2();
		}
		else
		{
			FindAndPrintClosestValues();
		}
	}

	private void dat16()
	{
		byte[] array = new byte[5] { 114, 5, 113, 22, 2 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		SendCommand(array, array.Length, ref reply, ref replySize);
		string text = BitConverter.ToString(reply, 0, (int)replySize);
		Console.WriteLine("Response 16: " + text);
		if (reply[1] == 5)
		{
			tek16 = false;
		}
	}

	private void dat17()
	{
		byte[] array = new byte[5] { 114, 5, 113, 23, 1 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		SendCommand(array, array.Length, ref reply, ref replySize);
		if (reply[1] == 5)
		{
			tek17 = false;
			return;
		}
		try
		{
			RPMD = (reply[4] << 8) + reply[5];
			TPSD = (float)(int)reply[7] / 2f;
			float num = ((reply[12] != byte.MaxValue) ? ((float)(int)reply[12] * 5f / 256f) : ((float)(int)reply[8] * 5f / 256f));
			CL = ((reply[13] != byte.MaxValue) ? (reply[13] - 40) : (reply[9] - 40));
			AD = (float)(int)reply[17] / 2f - 64f;
			inj = (float)((reply[15] << 8) + reply[16]) / 200f;
			IDC = (float)RPMD * inj / 1200f;
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error decoding data: " + ex.Message);
		}
		string text = BitConverter.ToString(reply, 0, (int)replySize);
		analogrpm();
		analogtps();
		dat20();
		hitunghp();
		hitungtq();
		if (liv)
		{
			FindAndPrintClosestValues2();
		}
		else
		{
			FindAndPrintClosestValues();
		}
	}

	private void dat20()
	{
		byte[] array = new byte[5] { 114, 5, 113, 32, 248 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		SendCommand(array, array.Length, ref reply, ref replySize);
		try
		{
			O2V = (float)(reply[4] & 0xFF) / 195.19519f;
			O2V2 = (float)(reply[4] & 0xFF) / 50.9937f;
			AFR = (float)(-5.351 * (double)O2V2 + 17.700000762939453);
			if (selectedStartAddress == ig1 || selectedStartAddress == ig2 || selectedStartAddress == ig3 || selectedStartAddress == ig4)
			{
				NILAI = (float)MapVoltageToValueig(O2V2);
			}
			else
			{
				NILAI = (float)MapVoltageToValue(O2V2);
			}
			float num = -100f;
			float num2 = 25f;
			nilaiPersen = (NILAI - num) / (num2 - num) * 100f;
			if (nilaiPersen < 0f)
			{
				nilaiPersen = 0f;
			}
			if (nilaiPersen > 100f)
			{
				nilaiPersen = 100f;
			}
		}
		catch (Exception)
		{
		}
	}

	public double MapVoltageToValue(double voltage)
	{
		double[] array = new double[5] { 0.0, 0.961, 1.922, 2.883, 3.844 };
		double[] array2 = new double[5] { 25.0, 0.0, -25.0, -50.0, -75.0 };
		if (voltage <= array[0])
		{
			return array2[0];
		}
		if (voltage >= array[4])
		{
			return array2[4];
		}
		for (int i = 0; i < array.Length - 1; i++)
		{
			if (voltage >= array[i] && voltage <= array[i + 1])
			{
				double num = (voltage - array[i]) / (array[i + 1] - array[i]);
				return array2[i] + num * (array2[i + 1] - array2[i]);
			}
		}
		return 0.0;
	}

	public double MapVoltageToValueig(double voltage)
	{
		double[] array = new double[5] { 0.0, 0.5, 1.0, 2.0, 3.0 };
		double[] array2 = new double[5] { 15.0, 0.0, 5.0, 10.0, 15.0 };
		if (voltage <= array[0])
		{
			return array2[0];
		}
		if (voltage >= array[4])
		{
			return array2[4];
		}
		for (int i = 0; i < array.Length - 1; i++)
		{
			if (voltage >= array[i] && voltage <= array[i + 1])
			{
				double num = (voltage - array[i]) / (array[i + 1] - array[i]);
				return array2[i] + num * (array2[i + 1] - array2[i]);
			}
		}
		return 0.0;
	}

	private void FindAndPrintClosestValues2()
	{
		Debug.WriteLine($"Original RPMD: {RPMD}, TPSD: {TPSD}");
		int num = FindClosestIndex(rpmValues, RPMD);
		Debug.WriteLine($"Closest RPM value: {rpmValues[num]} at index {num}");
		if (selectedStartAddress == iap)
		{
			int num2 = FindClosestIndex(mapValues, MAP);
			Debug.WriteLine($"Closest map value: {mapValues[num2]} at index {num2}");
			Debug.WriteLine($"Pointer is at RPM row: {num + 1}, map column: {num2 + 1}");
			pointerRow = num + 1;
			pointerColumn = num2 + 1;
		}
		else
		{
			int num3 = FindClosestIndex(tpsValues, TPSD);
			Debug.WriteLine($"Closest TPS value: {tpsValues[num3]} at index {num3}");
			Debug.WriteLine($"Pointer is at RPM row: {num + 1}, TPS column: {num3 + 1}");
			pointerRow = num + 1;
			pointerColumn = num3 + 1;
		}
		UpdatePointerPosition2();
	}

	private void StoreInitialCellValues()
	{
		for (int i = 0; i < dataGridView1.Rows.Count; i++)
		{
			for (int j = 0; j < dataGridView1.Columns.Count; j++)
			{
				if (double.TryParse(dataGridView1.Rows[i].Cells[j].Value?.ToString(), out var result))
				{
					initialCellValues[(i, j)] = result;
				}
			}
		}
	}

	private void UpdatePointerPosition2()
	{
		Rectangle cellDisplayRectangle = dataGridView1.GetCellDisplayRectangle(pointerColumn, pointerRow, cutOverflow: true);
		int num = pointerRow;
		int num2 = pointerColumn;
		pointerRow = FindClosestIndex(rpmValues, RPMD) + 1;
		if (selectedStartAddress == iap)
		{
			pointerColumn = FindClosestIndex(mapValues, MAP) + 1;
		}
		else
		{
			pointerColumn = FindClosestIndex(tpsValues, TPSD) + 1;
		}
		Rectangle cellDisplayRectangle2 = dataGridView1.GetCellDisplayRectangle(pointerColumn, pointerRow, cutOverflow: true);
		dataGridView1.Invalidate(cellDisplayRectangle);
		dataGridView1.Invalidate(cellDisplayRectangle2);
		if (pointerRow >= 0 && pointerRow < dataGridView1.Rows.Count && pointerColumn >= 0 && pointerColumn < dataGridView1.Columns.Count)
		{
			DataGridViewCell dataGridViewCell = dataGridView1.Rows[pointerRow].Cells[pointerColumn];
			if (initialCellValues.TryGetValue((pointerRow, pointerColumn), out var value))
			{
				Console.WriteLine($"Nilai awal di cell ({pointerRow}, {pointerColumn}): {value}");
				double num3 = NILAI / 100f;
				double value2 = value + value * num3 / 1.0;
				value2 = Math.Round(value2, 3);
				dataGridViewCell.Value = value2;
				Console.WriteLine($"percentage ({pointerRow}, {pointerColumn}): {num3}");
				Console.WriteLine($"Nilai baru di cell ({pointerRow}, {pointerColumn}): {value2}");
			}
			dataGridView1.Invalidate(cellDisplayRectangle2);
		}
	}

	private void UpdatePointerPosition()
	{
		Rectangle cellDisplayRectangle = dataGridView1.GetCellDisplayRectangle(pointerColumn, pointerRow, cutOverflow: true);
		Rectangle cellDisplayRectangle2 = dataGridView1.GetCellDisplayRectangle(pointerColumn, pointerRow, cutOverflow: true);
		dataGridView1.Invalidate(cellDisplayRectangle);
		dataGridView1.Invalidate(cellDisplayRectangle2);
	}

	private void analogrpm()
	{
	}

	private void analogtps()
	{
	}

	private void flashcount()
	{
		DialogResult dialogResult = MessageBox.Show("KUNCI KONTAK OFF - ON\r\nSelama 5 Detik\r\nUntuk Terhubung ke EEPROM", "Reset Count Shindengen", MessageBoxButtons.OK);
		byte[] array = new byte[4] { 254, 4, 114, 140 };
		byte[] array2 = new byte[5] { 114, 5, 0, 240, 153 };
		byte[] array3 = new byte[13]
		{
			145, 145, 13, 223, 158, 141, 154, 134, 144, 138,
			140, 155, 136
		};
		byte[] array4 = new byte[13]
		{
			145, 145, 13, 223, 146, 158, 134, 150, 139, 141,
			134, 192, 106
		};
		byte[] array5 = new byte[9] { 145, 145, 9, 65, 14, 0, 0, 0, 168 };
		byte[] reply = new byte[256];
		uint replySize = 0u;
		if (InitFtdiIso())
		{
			if (SendCommand(array, array.Length, ref reply, ref replySize))
			{
				string text = BitConverter.ToString(reply, 0, (int)replySize);
				Console.WriteLine("Response: " + text);
			}
			Thread.Sleep(150);
			if (SendCommand(array2, array2.Length, ref reply, ref replySize))
			{
				string text2 = BitConverter.ToString(reply, 0, (int)replySize);
				Console.WriteLine("Response: " + text2);
			}
			Thread.Sleep(150);
			if (SendCommand(array3, array3.Length, ref reply, ref replySize))
			{
				string text3 = BitConverter.ToString(reply, 0, (int)replySize);
				Console.WriteLine("Response: " + text3);
			}
			Thread.Sleep(150);
			if (SendCommand(array4, array4.Length, ref reply, ref replySize))
			{
				string text4 = BitConverter.ToString(reply, 0, (int)replySize);
				Console.WriteLine("Response2: " + text4);
			}
			Thread.Sleep(150);
			if (SendCommand(array5, array5.Length, ref reply, ref replySize))
			{
				string text5 = BitConverter.ToString(reply, 0, (int)replySize);
				Console.WriteLine("Response3: " + text5);
			}
			Thread.Sleep(150);
		}
		MessageBox.Show("Reset Count Berhasil", "Reset Count Shindengen", MessageBoxButtons.OK);
	}

	private async void LoopDat17(CancellationToken token)
	{
		while (!token.IsCancellationRequested)
		{
			if (tek11)
			{
				dat11();
			}
			if (tek10)
			{
				dat10();
			}
			if (tek13)
			{
				dat13();
			}
			if (tek16)
			{
				dat16();
			}
			if (tek17)
			{
				dat17();
			}
			try
			{
				await Task.Delay(50, token);
			}
			catch (TaskCanceledException)
			{
				break;
			}
			SetControlText(label7, RPMD.ToString());
			SetControlText(label6, TPSD.ToString("F1") + "°");
			SetControlText(label4, CL.ToString("F1") + "°C");
			SetControlText(label5, AD.ToString("F1") + "°");
			SetControlText(label8, inj.ToString("F3") + "  Ms");
			SetControlText(label2, AFR.ToString("F1") + "  :1");
			SetControlText(label9, IDC.ToString("F3"));
			SetControlText(label10, NILAI.ToString("F1"));
			SetControlText(label11, (cc > 0f) ? HP.ToString("F2") : "--");
			UpdateAfrDisplay();
		}
	}

	private void StopLoopDat17()
	{
		if (_cancellationTokenSource != null)
		{
			_cancellationTokenSource.Cancel();
		}
	}

	private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (ftHandle != IntPtr.Zero)
		{
			FTDI.FT_Close(ftHandle);
			ftHandle = IntPtr.Zero;
		}
	}

	private void CloseFtdiHandle()
	{
		if (ftHandle != IntPtr.Zero)
		{
			FTDI.FT_Close(ftHandle);
			ftHandle = IntPtr.Zero;
		}
	}

	private void button7_Click(object sender, EventArgs e)
	{
		try
		{
			FormMain_FormClosing(this, new FormClosingEventArgs(CloseReason.None, cancel: false));
			endThread = true;
			workerThread?.Join();
			Hide();
			Diag diag = new Diag();
			CloseFtdiHandle();
			diag.ShowDialog();
			Show();
			endThread = false;
		}
		catch (Exception ex)
		{
			MessageBox.Show("Error in button1_Click: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void button4_Click(object sender, EventArgs e)
	{
		StopLoopDat17();
	}

	private void button5_Click(object sender, EventArgs e)
	{
		Thread thread = new Thread(() =>
		{
			flashcount();
		})
		{
			IsBackground = true
		};
		thread.Start();
	}

	private void ignition4ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			button9.Enabled = true;
			selectedStartAddress = ig4;
			IGNITION1();
		}
	}

	private void limiter7ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			button9.Enabled = true;
			selectedStartAddress = limit7;
			limiter1();
		}
	}

	private void offfungsi()
	{
		breakpointTPSToolStripMenuItem.Visible = false;
		breakpoinRPMToolStripMenuItem.Visible = false;
		breakpointIAPToolStripMenuItem.Visible = false;
		toolStripMenuItem6.Visible = false;
		toolStripMenuItem7.Visible = false;
		toolStripMenuItem8.Visible = false;
		ignition4ToolStripMenuItem.Visible = false;
		ignition5ToolStripMenuItem.Visible = false;
		rpmVsTps1ToolStripMenuItem.Visible = false;
		rpmVsTps2ToolStripMenuItem.Visible = false;
		rpmVsToolStripMenuItem.Visible = false;
		rpmVsIAPToolStripMenuItem.Visible = false;
		toolStripMenuItem10.Visible = false;
		toolStripMenuItem11.Visible = false;
		toolStripMenuItem12.Visible = false;
		toolStripMenuItem13.Visible = false;
		toolStripMenuItem14.Visible = false;
		limiter6ToolStripMenuItem.Visible = false;
		limiter7ToolStripMenuItem.Visible = false;
		toolStripMenuItem16.Visible = false;
		toolStripMenuItem17.Visible = false;
		toolStripMenuItem18.Visible = false;
		toolStripMenuItem19.Visible = false;
		toolStripMenuItem20.Visible = false;
		toolStripMenuItem21.Visible = false;
		toolStripMenuItem22.Visible = false;
		offMode4ToolStripMenuItem.Visible = false;
		offBASSensorToolStripMenuItem.Visible = false;
		offMAPSensorToolStripMenuItem.Visible = false;
		offECTSensorToolStripMenuItem.Visible = false;
		offIATSensorToolStripMenuItem.Visible = false;
		offVSSSensorToolStripMenuItem.Visible = false;
		offMAPSensorToolStripMenuItem1.Visible = false;
	}

	private void Form1_Load(object sender, EventArgs e)
	{
		SetControlText(TxtPb, "⏳0%");
		Thread thread = new Thread(WorkerThread)
		{
			IsBackground = true
		};
		thread.Start();
		Visible = false;
		string text = GetProcessorId().Trim();
		Debug.WriteLine("Processor Serial Number: " + text);
		string text2 = GetDiskSerialNumber().Trim();
		Debug.WriteLine("Disk Serial Number: " + text2);
		string text3 = text + "_" + text2;
		Debug.WriteLine("Combined Serial Number: " + text3);
		string text4 = ProcessDiskSerial(text3);
		Debug.WriteLine("Processed Combined Serial Number: " + text4);
		string[] array = DecryptKeyFromFile();
		string text5 = array[0];
		string text6 = array[1];
		string text7 = array[2];
		Debug.WriteLine("Stored Key: " + text5);
		Debug.WriteLine("User Name: " + text6);
		Debug.WriteLine("Phone Number: " + text7);
		label3.Text = text6 + " (" + text7 + ")";
		if (string.IsNullOrEmpty(text6) || string.IsNullOrEmpty(text7) || text4 != text5)
		{
			string why = H2License.Diagnose(array, text4);
			if (why.Length == 0) why = "Nama / No. HP pada lisensi kosong.";
			MessageBox.Show(why + "\n\nSimpan HWID komputer ini pada dialog berikut bila perlu membuat lisensi baru.", "Lisensi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			ShowSaveFileDialog(text3);
			Application.Exit();
		}
		else
		{
			Visible = true;
		}
	}

	private static string GetDiskSerialNumber()
	{
		try
		{
			string text = string.Empty;
			ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_PhysicalMedia");
			foreach (ManagementObject item in managementObjectSearcher.Get())
			{
				text = item["SerialNumber"]?.ToString().Trim();
				if (!string.IsNullOrEmpty(text))
				{
					break;
				}
			}
			return text;
		}
		catch (Exception ex)
		{
			Debug.WriteLine("Error retrieving disk serial number: " + ex.Message);
			return "Error";
		}
	}

	private static string GetProcessorId()
	{
		string result = string.Empty;
		ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
		using (ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = managementObjectSearcher.Get().GetEnumerator())
		{
			if (managementObjectEnumerator.MoveNext())
			{
				ManagementObject managementObject = (ManagementObject)managementObjectEnumerator.Current;
				result = managementObject["ProcessorId"]?.ToString();
			}
		}
		return result;
	}

	private string ProcessDiskSerial(string serialNumber)
	{
		return H2License.HwidHash(serialNumber);
	}

	private string DecryptString(string encryptedText)
	{
		using Aes aes = Aes.Create();
		aes.Key = Key;
		aes.IV = IV;
		ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
		try
		{
			byte[] buffer = Convert.FromBase64String(encryptedText);
			using MemoryStream stream = new MemoryStream(buffer);
			using CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read);
			using StreamReader streamReader = new StreamReader(stream2);
			return streamReader.ReadToEnd();
		}
		catch (Exception ex)
		{
			Debug.WriteLine("Error decrypting string: " + ex.Message);
			return "Error";
		}
	}

	private string[] DecryptKeyFromFile()
	{
		return H2License.Load("license.lic");
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == (Keys.I | Keys.Control))
		{
			ShowUserInfo();
			return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	private void ShowUserInfo()
	{
		string[] array = label14.Text.Split(',');
		string text = ((array.Length != 0) ? array[0].Trim() : "Unknown User");
		string text2 = ((array.Length > 1) ? array[1].Trim() : "Unknown Number");
		string text3 = "User ID:  " + text + "\nNumber Phone:  " + text2;
		MessageBox.Show(text3, "User Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	private void ShowSaveFileDialog(string combinedSerialNumber)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
			Title = "Save Serial Number"
		};
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			try
			{
				File.WriteAllText(saveFileDialog.FileName, combinedSerialNumber);
				Debug.WriteLine("Saved serial number to " + saveFileDialog.FileName);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error saving file: " + ex.Message);
			}
		}
	}

	private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
	{
		if (dataGridView1 != null && dataGridView1.Rows.Count > 0 && !dataGridView1.IsCurrentCellInEditMode)
		{
			if (e.Control && e.KeyCode == Keys.Z)
			{
				if (binBytes != null)
				{
					Undo();
				}
				e.SuppressKeyPress = true;
				e.Handled = true;
				return;
			}
			if (e.Control && e.KeyCode == Keys.Y)
			{
				if (binBytes != null)
				{
					Redo();
				}
				e.SuppressKeyPress = true;
				e.Handled = true;
				return;
			}
			int adjustSign = 0;
			bool adjustBig = false;
			if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus)
			{
				adjustSign = 1;
			}
			else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
			{
				adjustSign = -1;
			}
			else if (e.KeyCode == Keys.PageUp)
			{
				adjustSign = 1;
				adjustBig = true;
			}
			else if (e.KeyCode == Keys.PageDown)
			{
				adjustSign = -1;
				adjustBig = true;
			}
			if (adjustSign != 0 && binBytes != null)
			{
				AdjustSelectedCells(adjustSign, e.Control && !adjustBig, adjustBig);
				e.SuppressKeyPress = true;
				e.Handled = true;
				return;
			}
		}
		if (!e.Control || e.KeyCode != Keys.G || dataGridView1 == null || dataGridView1.Rows.Count == 0)
		{
			return;
		}
		Point point = dataGridView1.PointToClient(Cursor.Position);
		DataGridView.HitTestInfo hitTestInfo = dataGridView1.HitTest(point.X, point.Y);
		if (hitTestInfo != null && hitTestInfo.RowIndex >= 0)
		{
			selectedRowIndex = hitTestInfo.RowIndex;
			List<double> list = new List<double>();
			foreach (DataGridViewCell cell in dataGridView1.Rows[hitTestInfo.RowIndex].Cells)
			{
				if (cell.Value != null && double.TryParse(cell.Value.ToString(), out var result))
				{
					list.Add(result);
				}
			}
			if (list.Count > 0)
			{
				form2d = new _2d();
				form2d.DataPointValueChanged += Form2d_DataPointValueChanged;
				form2d.ShowChart(list);
				form2d.Show();
			}
			else
			{
				MessageBox.Show("Tidak ada nilai yang valid untuk ditampilkan dalam grafik.");
			}
		}
		else
		{
			MessageBox.Show("Tidak ada baris yang dipilih.");
		}
		e.SuppressKeyPress = true;
	}

	private void Form2d_DataPointValueChanged(int index, double newValue)
	{
		if (selectedRowIndex >= 0 && selectedRowIndex < dataGridView1.Rows.Count && index >= 0 && index < dataGridView1.Columns.Count)
		{
			dataGridView1.Rows[selectedRowIndex].Cells[index].Value = newValue;
		}
	}

	private void Form1_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Control && e.KeyCode == Keys.C)
		{
			CopySelectedCells();
		}
		else if (e.Control && e.KeyCode == Keys.V)
		{
			PasteDataToSelectedCells();
		}
	}

	private void CopySelectedCells()
	{
		if (dataGridView1.SelectedCells.Count <= 0)
		{
			return;
		}
		List<DataGridViewCell> list = (from DataGridViewCell cell in dataGridView1.SelectedCells
			orderby cell.RowIndex, cell.ColumnIndex
			select cell).ToList();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (DataGridViewCell item in list)
		{
			stringBuilder.Append(item.Value?.ToString() ?? string.Empty);
			stringBuilder.Append("\t");
		}
		Clipboard.SetText(stringBuilder.ToString().TrimEnd('\t'));
	}

	private void PasteDataToSelectedCells()
	{
		if (dataGridView1.SelectedCells.Count <= 0)
		{
			return;
		}
		string text = Clipboard.GetText();
		string[] array = text.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		List<DataGridViewCell> list = (from DataGridViewCell cell in dataGridView1.SelectedCells
			orderby cell.RowIndex, cell.ColumnIndex
			select cell).ToList();
		List<string> list2 = new List<string>();
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			string[] collection = text2.Split('\t');
			list2.AddRange(collection);
		}
		if (list.Count != list2.Count)
		{
			MessageBox.Show("Jumlah sel yang dipilih tidak sesuai dengan jumlah data yang disalin.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		int num2 = 0;
		foreach (DataGridViewCell item in list)
		{
			if (num2 < list2.Count)
			{
				dataGridView1.CurrentCell = item;
				dataGridView1.BeginEdit(selectAll: true);
				item.Value = list2[num2];
				num2++;
				continue;
			}
			break;
		}
		dataGridView1.EndEdit();
	}

	private void hitungcc()
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		string s = textBox1.Text.Replace(',', '.');
		string s2 = textBox2.Text.Replace(',', '.');
		if (float.TryParse(s, NumberStyles.Float, invariantCulture, out var result) && float.TryParse(s2, NumberStyles.Float, invariantCulture, out var result2))
		{
			cc = 3.14f * (result / 2f * (result / 2f)) * result2 / 1000f;
			SetControlText(label13, cc.ToString("F3") + "  cm3");
		}
		else
		{
			MessageBox.Show("Mohon masukkan angka yang valid untuk BORE dan STROKE.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void hitunghp()
	{
		if (cc <= 0f)
		{
			TryAutoCc();
		}
		if (cc > 0f)
		{
			HP = Math.Max(0f, cc * 0.0985f + (float)RPMD * 0.00172f - 14.7f);
		}
	}

	private void hitungtq()
	{
		if (cc > 0f)
		{
			TQ = cc * 0.0985f * 5252f / (float)RPMD + 2.076f;
		}
	}

	private void button10_Click(object sender, EventArgs e)
	{
		hitungcc();
	}

	private void InitializeContextMenu()
	{
		ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
		contextMenuStrip.Items.Add("Copy", null, (object s, EventArgs e) =>
		{
			CopySelectedCells();
		});
		contextMenuStrip.Items.Add("Paste", null, (object s, EventArgs e) =>
		{
			PasteDataToSelectedCells();
		});
		dataGridView1.ContextMenuStrip = contextMenuStrip;
		KeyDown += Form1_KeyDown;
	}

	private void button4_Click_1(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			Undo();
		}
	}

	private void button6_Click(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			Redo();
		}
	}

	private void button8_Click(object sender, EventArgs e)
	{
		if (binBytes == null)
		{
			return;
		}
		liv = true;
		ukuranpointer = 15;
		StoreInitialCellValues();
		if (!isTaskRunning)
		{
			showPointer = !showPointer;
			dataGridView1.Invalidate();
			_cancellationTokenSource = new CancellationTokenSource();
			InitFtdiIso();
			Thread thread = new Thread(() =>
			{
				LoopDat17(_cancellationTokenSource.Token);
			})
			{
				IsBackground = true
			};
			thread.Start();
			isTaskRunning = true;
			button8.Text = "Stop";
		}
		else
		{
			showPointer = !showPointer;
			dataGridView1.Invalidate();
			_cancellationTokenSource.Cancel();
			isTaskRunning = false;
			liv = false;
			button8.Text = "Auto Map";
		}
	}

	private void offBASSensorToolStripMenuItem_Click(object sender, EventArgs e)
	{
		button9.Enabled = false;
		O2(bass, bassmask);
	}

	private void offMAPSensorToolStripMenuItem_Click(object sender, EventArgs e)
	{
		button9.Enabled = false;
		O2(mapp, mappmask);
	}

	private void offECTSensorToolStripMenuItem_Click(object sender, EventArgs e)
	{
		button9.Enabled = false;
		O2(ect, ectmask);
	}

	private void offIATSensorToolStripMenuItem_Click(object sender, EventArgs e)
	{
		button9.Enabled = false;
		O2(iat, iatmask);
	}

	private void offVSSSensorToolStripMenuItem_Click(object sender, EventArgs e)
	{
		button9.Enabled = false;
		O2(vss, vssmask);
	}

	private void offMAPSensorToolStripMenuItem1_Click(object sender, EventArgs e)
	{
		button9.Enabled = false;
		O2(iacv, iacvmask);
	}

	private void ignition5ToolStripMenuItem_Click(object sender, EventArgs e)
	{
	}

	private void button12_Click(object sender, EventArgs e)
	{
		if (binBytes == null)
		{
			return;
		}
		ukuranpointer = 10;
		StoreInitialCellValues();
		if (!isTaskRunning)
		{
			showPointer = !showPointer;
			dataGridView1.Invalidate();
			_cancellationTokenSource = new CancellationTokenSource();
			InitFtdiIso();
			Thread thread = new Thread(() =>
			{
				LoopDat17(_cancellationTokenSource.Token);
			})
			{
				IsBackground = true
			};
			thread.Start();
			if (dynoForm == null || dynoForm.IsDisposed)
			{
				dynoForm = new DYNO();
				dynoForm.FormClosed += DynoForm_FormClosed;
			}
			dynoForm.Show();
			updateTimer = new System.Windows.Forms.Timer();
			updateTimer.Interval = 50;
			updateTimer.Tick += (object s, EventArgs args) =>
			{
				if (dynoForm != null)
				{
					dynoForm.RPMD = RPMD;
					dynoForm.HP = HP;
					dynoForm.AFR = AFR;
					dynoForm.TQ = TQ;
					dynoForm.Updatedyno(RPMD, HP, AFR, TQ);
				}
			};
			updateTimer.Start();
			isTaskRunning = true;
		}
		else
		{
			showPointer = !showPointer;
			dataGridView1.Invalidate();
			if (dynoForm != null && !dynoForm.IsDisposed)
			{
				dynoForm.Close();
				dynoForm.Dispose();
				dynoForm = null;
			}
			_cancellationTokenSource.Cancel();
			isTaskRunning = false;
			if (updateTimer != null)
			{
				updateTimer.Stop();
				updateTimer.Dispose();
			}
		}
	}

	private void DynoForm_FormClosed(object sender, FormClosedEventArgs e)
	{
		if (updateTimer != null)
		{
			updateTimer.Stop();
			updateTimer.Dispose();
			updateTimer = null;
		}
		if (_cancellationTokenSource != null)
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
			_cancellationTokenSource = null;
		}
		isTaskRunning = false;
		dynoForm = null;
		showPointer = !showPointer;
		dataGridView1.Invalidate();
	}

	private void toolStripMenuItem1_Click(object sender, EventArgs e)
	{
	}

	private void button13_Click(object sender, EventArgs e)
	{
	}

	private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
	{
	}

	private void breakpointToolStripMenuItem1_Click(object sender, EventArgs e)
	{
	}

	private void breakpoinRPMToolStripMenuItem_Click_1(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			button9.Enabled = true;
			selectedStartAddress = rpmh;
			RPMBREAKPOIN();
		}
	}

	private void breakpointTPSToolStripMenuItem_Click_1(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			button9.Enabled = true;
			selectedStartAddress = tpsh;
			TPSBREAK();
		}
	}

	private void breakpointIAPToolStripMenuItem_Click_1(object sender, EventArgs e)
	{
		if (binBytes != null)
		{
			button9.Enabled = true;
			selectedStartAddress = iaph;
			IAPBREAK();
		}
	}

	private void label1_Click(object sender, EventArgs e)
	{
	}

	private void label14_Click(object sender, EventArgs e)
	{
	}

	private void isieditvalue()
	{
		comboBox2.Items.Add("Offset");
		comboBox2.Items.Add("Offset (+/-)");
		comboBox2.Items.Add("Scala by %");
		comboBox2.Items.Add("Smooth");
		comboBox2.Items.Add("Copy from Compare");
		if (comboBox2.Items.Count > 0)
		{
			comboBox2.SelectedIndex = 0;
		}
	}

	private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
	{
	}

	private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
	{
	}

	private void readIDBarcodeToolStripMenuItem_Click(object sender, EventArgs e)
	{
		SMK sMK = new SMK();
		sMK.Show();
	}

	private void dToolStripMenuItem_Click(object sender, EventArgs e)
	{
		_2d _2d2 = new _2d();
		_2d2.Show();
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		teks = teks.Substring(1) + teks[0];
		Text = teks;
	}

	private void timer2_Tick(object sender, EventArgs e)
	{
		isivom();
	}

	private void menuStrip7_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(_3D_Chart_Graph.Form1));
		this.show_all = new System.Windows.Forms.Button();
		this.button2 = new System.Windows.Forms.Button();
		this.txtInput = new System.Windows.Forms.TextBox();
		this.TbFileName = new System.Windows.Forms.TextBox();
		this.button3 = new System.Windows.Forms.Button();
		this.CbFastWrite = new System.Windows.Forms.CheckBox();
		this.TbStartOffset = new System.Windows.Forms.TextBox();
		this.TbChecksumOffset = new System.Windows.Forms.TextBox();
		this.button12 = new System.Windows.Forms.Button();
		this.button8 = new System.Windows.Forms.Button();
		this.button1 = new System.Windows.Forms.Button();
		this.BtnWrite = new System.Windows.Forms.Button();
		this.panel4 = new System.Windows.Forms.Panel();
		this.label3 = new System.Windows.Forms.Label();
		this.TxtPb = new System.Windows.Forms.Label();
		this.label18 = new System.Windows.Forms.Label();
		this.label17 = new System.Windows.Forms.Label();
		this.TxtEcmId = new System.Windows.Forms.Label();
		this.TxtFlashCount = new System.Windows.Forms.Label();
		this.TxtConnStat = new System.Windows.Forms.Label();
		this.PbProgress = new System.Windows.Forms.ProgressBar();
		this.label14 = new System.Windows.Forms.Label();
		this.comboBox1 = new System.Windows.Forms.ComboBox();
		this.TxtPartCode = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
		this.label11 = new System.Windows.Forms.Label();
		this.label8 = new System.Windows.Forms.Label();
		this.menuStrip7 = new System.Windows.Forms.MenuStrip();
		this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.button9 = new System.Windows.Forms.ToolStripMenuItem();
		this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.diagnosticToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.resetCountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.keihinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.shindengenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.breakpointToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.readIDBarcodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.convertHexToIDKeyYamahaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.unlockPasswordXdfToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem23 = new System.Windows.Forms.ToolStripMenuItem();
		this.breakpointToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
		this.breakpoinRPMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.breakpointTPSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.breakpointIAPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.fueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.rpmVsTps1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.rpmVsTps2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.rpmVsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.rpmVsIAPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.ignitionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
		this.ignition4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.ignition5ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.limiterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem12 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem14 = new System.Windows.Forms.ToolStripMenuItem();
		this.limiter6ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.limiter7ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.settinganLaiinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem16 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem17 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem18 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem19 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem20 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem21 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem22 = new System.Windows.Forms.ToolStripMenuItem();
		this.offMode4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.offBASSensorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.offMAPSensorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.offECTSensorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.offIATSensorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.offVSSSensorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.offMAPSensorToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.comboBox2 = new System.Windows.Forms.ComboBox();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.groupBox3 = new System.Windows.Forms.GroupBox();
		this.groupBox4 = new System.Windows.Forms.GroupBox();
		this.groupBox5 = new System.Windows.Forms.GroupBox();
		this.groupBox6 = new System.Windows.Forms.GroupBox();
		this.label15 = new System.Windows.Forms.Label();
		this.label16 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		this.label19 = new System.Windows.Forms.Label();
		this.label20 = new System.Windows.Forms.Label();
		this.groupBox7 = new System.Windows.Forms.GroupBox();
		this.timer2 = new System.Windows.Forms.Timer(this.components);
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.label2 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.label10 = new System.Windows.Forms.Label();
		this.button4 = new System.Windows.Forms.Button();
		this.button6 = new System.Windows.Forms.Button();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.textBox2 = new System.Windows.Forms.TextBox();
		this.label12 = new System.Windows.Forms.Label();
		this.label13 = new System.Windows.Forms.Label();
		this.button10 = new System.Windows.Forms.Button();
		this.panelDATA = new System.Windows.Forms.Panel();
		this.panel4.SuspendLayout();
		this.menuStrip7.SuspendLayout();
		this.groupBox1.SuspendLayout();
		this.groupBox2.SuspendLayout();
		this.groupBox3.SuspendLayout();
		this.groupBox4.SuspendLayout();
		this.groupBox5.SuspendLayout();
		this.groupBox6.SuspendLayout();
		this.groupBox7.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		this.panelDATA.SuspendLayout();
		base.SuspendLayout();
		this.show_all.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.show_all.BackColor = System.Drawing.Color.Black;
		this.show_all.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.show_all.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.show_all.ForeColor = System.Drawing.Color.Cyan;
		this.show_all.Location = new System.Drawing.Point(648, 119);
		this.show_all.Name = "show_all";
		this.show_all.Size = new System.Drawing.Size(72, 32);
		this.show_all.TabIndex = 2;
		this.show_all.Text = "\ud83d\udccaGraph";
		this.show_all.UseVisualStyleBackColor = false;
		this.show_all.Click += new System.EventHandler(this.show_all_Click);
		this.button2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.button2.BackColor = System.Drawing.Color.DimGray;
		this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.button2.ForeColor = System.Drawing.Color.Cyan;
		this.button2.Location = new System.Drawing.Point(226, 9);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(56, 24);
		this.button2.TabIndex = 23;
		this.button2.Text = "Execute";
		this.button2.UseVisualStyleBackColor = false;
		this.button2.Click += new System.EventHandler(this.btnOK_Click_1);
		this.txtInput.BackColor = System.Drawing.Color.Black;
		this.txtInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.txtInput.ForeColor = System.Drawing.Color.Cyan;
		this.txtInput.Location = new System.Drawing.Point(185, 8);
		this.txtInput.Multiline = true;
		this.txtInput.Name = "txtInput";
		this.txtInput.Size = new System.Drawing.Size(35, 24);
		this.txtInput.TabIndex = 20;
		this.txtInput.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.TbFileName.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.TbFileName.BackColor = System.Drawing.Color.Black;
		this.TbFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TbFileName.ForeColor = System.Drawing.Color.Cyan;
		this.TbFileName.Location = new System.Drawing.Point(52, 122);
		this.TbFileName.Multiline = true;
		this.TbFileName.Name = "TbFileName";
		this.TbFileName.Size = new System.Drawing.Size(103, 29);
		this.TbFileName.TabIndex = 13;
		this.button3.BackColor = System.Drawing.Color.Black;
		this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.button3.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.button3.ForeColor = System.Drawing.Color.Cyan;
		this.button3.Location = new System.Drawing.Point(8, 637);
		this.button3.Name = "button3";
		this.button3.Size = new System.Drawing.Size(121, 37);
		this.button3.TabIndex = 15;
		this.button3.Text = "\ud83d\udd0cConnect ECM";
		this.button3.UseVisualStyleBackColor = false;
		this.button3.Click += new System.EventHandler(this.button3_Click);
		this.CbFastWrite.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.CbFastWrite.AutoSize = true;
		this.CbFastWrite.Checked = true;
		this.CbFastWrite.CheckState = System.Windows.Forms.CheckState.Checked;
		this.CbFastWrite.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.CbFastWrite.ForeColor = System.Drawing.Color.Cyan;
		this.CbFastWrite.Location = new System.Drawing.Point(891, 126);
		this.CbFastWrite.Name = "CbFastWrite";
		this.CbFastWrite.Size = new System.Drawing.Size(52, 20);
		this.CbFastWrite.TabIndex = 15;
		this.CbFastWrite.Text = "Fast";
		this.CbFastWrite.UseVisualStyleBackColor = true;
		this.TbStartOffset.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.TbStartOffset.BackColor = System.Drawing.Color.Black;
		this.TbStartOffset.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TbStartOffset.ForeColor = System.Drawing.Color.Cyan;
		this.TbStartOffset.Location = new System.Drawing.Point(1020, 119);
		this.TbStartOffset.Multiline = true;
		this.TbStartOffset.Name = "TbStartOffset";
		this.TbStartOffset.Size = new System.Drawing.Size(77, 29);
		this.TbStartOffset.TabIndex = 3;
		this.TbStartOffset.Text = "Auto";
		this.TbStartOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.TbChecksumOffset.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.TbChecksumOffset.BackColor = System.Drawing.Color.Black;
		this.TbChecksumOffset.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TbChecksumOffset.ForeColor = System.Drawing.Color.Cyan;
		this.TbChecksumOffset.Location = new System.Drawing.Point(1194, 119);
		this.TbChecksumOffset.Multiline = true;
		this.TbChecksumOffset.Name = "TbChecksumOffset";
		this.TbChecksumOffset.Size = new System.Drawing.Size(77, 29);
		this.TbChecksumOffset.TabIndex = 4;
		this.TbChecksumOffset.Text = "Auto";
		this.TbChecksumOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.button12.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.button12.BackColor = System.Drawing.Color.Black;
		this.button12.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.button12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.button12.ForeColor = System.Drawing.Color.Cyan;
		this.button12.Location = new System.Drawing.Point(822, 120);
		this.button12.Name = "button12";
		this.button12.Size = new System.Drawing.Size(55, 31);
		this.button12.TabIndex = 20;
		this.button12.Text = "▶\ufe0f Run";
		this.button12.UseVisualStyleBackColor = false;
		this.button12.Click += new System.EventHandler(this.button12_Click);
		this.button8.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.button8.BackColor = System.Drawing.Color.Black;
		this.button8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.button8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.button8.ForeColor = System.Drawing.Color.Cyan;
		this.button8.Location = new System.Drawing.Point(726, 120);
		this.button8.Name = "button8";
		this.button8.Size = new System.Drawing.Size(90, 31);
		this.button8.TabIndex = 15;
		this.button8.Text = "\ud83d\udd04Auto Map";
		this.button8.UseVisualStyleBackColor = false;
		this.button8.Click += new System.EventHandler(this.button8_Click);
		this.button1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.button1.BackColor = System.Drawing.Color.Black;
		this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.button1.ForeColor = System.Drawing.Color.Cyan;
		this.button1.Location = new System.Drawing.Point(558, 119);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(84, 32);
		this.button1.TabIndex = 3;
		this.button1.Text = "\ud83d\udcc8Live Data";
		this.button1.UseVisualStyleBackColor = false;
		this.button1.Click += new System.EventHandler(this.button1_Click);
		this.BtnWrite.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.BtnWrite.BackColor = System.Drawing.Color.Black;
		this.BtnWrite.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.BtnWrite.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.BtnWrite.ForeColor = System.Drawing.Color.Cyan;
		this.BtnWrite.Location = new System.Drawing.Point(475, 119);
		this.BtnWrite.Name = "BtnWrite";
		this.BtnWrite.Size = new System.Drawing.Size(77, 32);
		this.BtnWrite.TabIndex = 11;
		this.BtnWrite.Text = "⚡FLASH";
		this.BtnWrite.UseVisualStyleBackColor = false;
		this.BtnWrite.Click += new System.EventHandler(this.BtnWrite_Click_1);
		this.panel4.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.panel4.BackColor = System.Drawing.Color.Black;
		this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel4.Controls.Add(this.label3);
		this.panel4.Controls.Add(this.TxtPb);
		this.panel4.Controls.Add(this.label17);
		this.panel4.Controls.Add(this.TxtEcmId);
		this.panel4.Controls.Add(this.TxtFlashCount);
		this.panel4.Controls.Add(this.TxtConnStat);
		this.panel4.Controls.Add(this.PbProgress);
		this.panel4.Controls.Add(this.label14);
		this.panel4.Controls.Add(this.label18);
		this.panel4.Location = new System.Drawing.Point(135, 637);
		this.panel4.Name = "panel4";
		this.panel4.Size = new System.Drawing.Size(968, 37);
		this.panel4.TabIndex = 12;
		this.label3.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.label3.BackColor = System.Drawing.Color.Black;
		this.label3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label3.ForeColor = System.Drawing.Color.Cyan;
		this.label3.Location = new System.Drawing.Point(713, -1);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(253, 37);
		this.label3.TabIndex = 21;
		this.label3.Text = "label3";
		this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.TxtPb.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.TxtPb.BackColor = System.Drawing.Color.Black;
		this.TxtPb.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.TxtPb.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TxtPb.ForeColor = System.Drawing.Color.Cyan;
		this.TxtPb.Location = new System.Drawing.Point(658, 3);
		this.TxtPb.Name = "TxtPb";
		this.TxtPb.Size = new System.Drawing.Size(58, 27);
		this.TxtPb.TabIndex = 20;
		this.TxtPb.Text = "⏳100%";
		this.TxtPb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label18.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.label18.BackColor = System.Drawing.Color.Black;
		this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label18.ForeColor = System.Drawing.Color.Cyan;
		this.label18.Location = new System.Drawing.Point(397, 7);
		this.label18.Name = "label18";
		this.label18.Size = new System.Drawing.Size(116, 23);
		this.label18.TabIndex = 19;
		this.label18.Text = "FLASH COUNT :";
		this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label17.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.label17.BackColor = System.Drawing.Color.Black;
		this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label17.ForeColor = System.Drawing.Color.Cyan;
		this.label17.Location = new System.Drawing.Point(179, 4);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(76, 23);
		this.label17.TabIndex = 18;
		this.label17.Text = "ECM ID :";
		this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.TxtEcmId.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.TxtEcmId.BackColor = System.Drawing.Color.Black;
		this.TxtEcmId.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TxtEcmId.ForeColor = System.Drawing.Color.Cyan;
		this.TxtEcmId.Location = new System.Drawing.Point(261, 5);
		this.TxtEcmId.Name = "TxtEcmId";
		this.TxtEcmId.Size = new System.Drawing.Size(102, 23);
		this.TxtEcmId.TabIndex = 17;
		this.TxtEcmId.Text = "-";
		this.TxtEcmId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.TxtFlashCount.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.TxtFlashCount.BackColor = System.Drawing.Color.Black;
		this.TxtFlashCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TxtFlashCount.ForeColor = System.Drawing.Color.Cyan;
		this.TxtFlashCount.Location = new System.Drawing.Point(481, 5);
		this.TxtFlashCount.Name = "TxtFlashCount";
		this.TxtFlashCount.Size = new System.Drawing.Size(32, 23);
		this.TxtFlashCount.TabIndex = 5;
		this.TxtFlashCount.Text = "-";
		this.TxtFlashCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.TxtConnStat.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.TxtConnStat.BackColor = System.Drawing.Color.Black;
		this.TxtConnStat.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TxtConnStat.ForeColor = System.Drawing.Color.Cyan;
		this.TxtConnStat.Location = new System.Drawing.Point(5, 5);
		this.TxtConnStat.Name = "TxtConnStat";
		this.TxtConnStat.Size = new System.Drawing.Size(190, 23);
		this.TxtConnStat.TabIndex = 2;
		this.TxtConnStat.Text = "-";
		this.TxtConnStat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.PbProgress.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.PbProgress.ForeColor = System.Drawing.Color.FromArgb(255, 128, 0);
		this.PbProgress.Location = new System.Drawing.Point(519, 3);
		this.PbProgress.Maximum = 10000;
		this.PbProgress.Name = "PbProgress";
		this.PbProgress.Size = new System.Drawing.Size(133, 27);
		this.PbProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
		this.PbProgress.TabIndex = 6;
		this.label14.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.label14.BackColor = System.Drawing.Color.Black;
		this.label14.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label14.ForeColor = System.Drawing.Color.Yellow;
		this.label14.Location = new System.Drawing.Point(713, -2);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(253, 37);
		this.label14.TabIndex = 16;
		this.label14.Text = "label14";
		this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label14.Click += new System.EventHandler(this.label14_Click);
		this.comboBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.comboBox1.BackColor = System.Drawing.Color.Black;
		this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.comboBox1.ForeColor = System.Drawing.Color.Cyan;
		this.comboBox1.FormattingEnabled = true;
		this.comboBox1.Location = new System.Drawing.Point(8, 84);
		this.comboBox1.Name = "comboBox1";
		this.comboBox1.Size = new System.Drawing.Size(291, 24);
		this.comboBox1.TabIndex = 0;
		this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged_1);
		this.TxtPartCode.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.TxtPartCode.BackColor = System.Drawing.Color.Black;
		this.TxtPartCode.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.TxtPartCode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.TxtPartCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TxtPartCode.ForeColor = System.Drawing.Color.Cyan;
		this.TxtPartCode.Location = new System.Drawing.Point(8, 33);
		this.TxtPartCode.Name = "TxtPartCode";
		this.TxtPartCode.Size = new System.Drawing.Size(291, 47);
		this.TxtPartCode.TabIndex = 14;
		this.TxtPartCode.Text = "ECM PART CODE";
		this.TxtPartCode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label7.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label7.BackColor = System.Drawing.Color.Black;
		this.label7.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.label7.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 39.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label7.Location = new System.Drawing.Point(6, 18);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(136, 54);
		this.label7.TabIndex = 14;
		this.label7.Text = "0";
		this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label6.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label6.BackColor = System.Drawing.Color.Black;
		this.label6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.label6.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 39.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label6.Location = new System.Drawing.Point(6, 18);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(136, 54);
		this.label6.TabIndex = 13;
		this.label6.Text = "0";
		this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label5.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label5.BackColor = System.Drawing.Color.Black;
		this.label5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.label5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 39.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label5.Location = new System.Drawing.Point(6, 18);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(136, 54);
		this.label5.TabIndex = 13;
		this.label5.Text = "0";
		this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label4.BackColor = System.Drawing.Color.Black;
		this.label4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.label4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 39.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label4.Location = new System.Drawing.Point(6, 18);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(136, 54);
		this.label4.TabIndex = 12;
		this.label4.Text = "0";
		this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.saveFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog1_FileOk);
		this.label11.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label11.BackColor = System.Drawing.Color.Black;
		this.label11.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.label11.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 39.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label11.Location = new System.Drawing.Point(6, 18);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(136, 54);
		this.label11.TabIndex = 19;
		this.label11.Text = "0";
		this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label8.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label8.BackColor = System.Drawing.Color.Black;
		this.label8.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.label8.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 39.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label8.Location = new System.Drawing.Point(6, 18);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(136, 54);
		this.label8.TabIndex = 17;
		this.label8.Text = "0";
		this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.menuStrip7.Items.AddRange(new System.Windows.Forms.ToolStripItem[9] { this.fileToolStripMenuItem, this.diagnosticToolStripMenuItem, this.resetCountToolStripMenuItem, this.breakpointToolStripMenuItem1, this.breakpointToolStripMenuItem2, this.fueToolStripMenuItem, this.ignitionToolStripMenuItem, this.limiterToolStripMenuItem, this.settinganLaiinToolStripMenuItem });
		this.menuStrip7.Location = new System.Drawing.Point(0, 0);
		this.menuStrip7.Name = "menuStrip7";
		this.menuStrip7.Size = new System.Drawing.Size(1280, 25);
		this.menuStrip7.TabIndex = 16;
		this.menuStrip7.Text = "menuStrip7";
		this.menuStrip7.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip7_ItemClicked);
		this.fileToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.openFileToolStripMenuItem, this.button9, this.saveAsToolStripMenuItem });
		this.fileToolStripMenuItem.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
		this.fileToolStripMenuItem.Size = new System.Drawing.Size(57, 21);
		this.fileToolStripMenuItem.Text = "\ud83d\udcc1File";
		this.openFileToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
		this.openFileToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
		this.openFileToolStripMenuItem.Text = "\ud83d\udcc4 Open File";
		this.openFileToolStripMenuItem.Click += new System.EventHandler(this.button2_Click);
		this.button9.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.button9.Name = "button9";
		this.button9.Size = new System.Drawing.Size(146, 22);
		this.button9.Text = "\ud83d\udcbe Save";
		this.button9.Click += new System.EventHandler(this.button9_Click);
		this.saveAsToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
		this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
		this.saveAsToolStripMenuItem.Text = "\ud83d\udcbe Save As";
		this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.button11_Click);
		this.diagnosticToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.diagnosticToolStripMenuItem.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.diagnosticToolStripMenuItem.Name = "diagnosticToolStripMenuItem";
		this.diagnosticToolStripMenuItem.Size = new System.Drawing.Size(97, 21);
		this.diagnosticToolStripMenuItem.Text = "\ud83d\udcbbDiagnostic";
		this.diagnosticToolStripMenuItem.Click += new System.EventHandler(this.button7_Click);
		this.resetCountToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.resetCountToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.keihinToolStripMenuItem, this.shindengenToolStripMenuItem });
		this.resetCountToolStripMenuItem.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.resetCountToolStripMenuItem.Name = "resetCountToolStripMenuItem";
		this.resetCountToolStripMenuItem.Size = new System.Drawing.Size(106, 21);
		this.resetCountToolStripMenuItem.Text = "\ud83d\udd04Reset Count";
		this.keihinToolStripMenuItem.Name = "keihinToolStripMenuItem";
		this.keihinToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
		this.keihinToolStripMenuItem.Text = "keihin";
		this.keihinToolStripMenuItem.Click += new System.EventHandler(this.KeihinResetCount_Click);
		this.shindengenToolStripMenuItem.Name = "shindengenToolStripMenuItem";
		this.shindengenToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
		this.shindengenToolStripMenuItem.Text = "Shindengen";
		this.shindengenToolStripMenuItem.Click += new System.EventHandler(this.button5_Click);
		this.breakpointToolStripMenuItem1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.breakpointToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.readIDBarcodeToolStripMenuItem });
		this.breakpointToolStripMenuItem1.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.breakpointToolStripMenuItem1.Name = "breakpointToolStripMenuItem1";
		this.breakpointToolStripMenuItem1.Size = new System.Drawing.Size(61, 21);
		this.breakpointToolStripMenuItem1.Text = "\ud83d\udee0\ufe0fTool";
		this.breakpointToolStripMenuItem1.Click += new System.EventHandler(this.breakpointToolStripMenuItem1_Click);
		this.readIDBarcodeToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.readIDBarcodeToolStripMenuItem.Name = "readIDBarcodeToolStripMenuItem";
		this.readIDBarcodeToolStripMenuItem.Size = new System.Drawing.Size(266, 22);
		this.readIDBarcodeToolStripMenuItem.Text = "\ud83d\udd11 Read ID Barcode";
		this.readIDBarcodeToolStripMenuItem.Click += new System.EventHandler(this.readIDBarcodeToolStripMenuItem_Click);
		this.convertHexToIDKeyYamahaToolStripMenuItem.Name = "convertHexToIDKeyYamahaToolStripMenuItem";
		this.convertHexToIDKeyYamahaToolStripMenuItem.Size = new System.Drawing.Size(266, 22);
		this.convertHexToIDKeyYamahaToolStripMenuItem.Text = "\ud83d\udce4 Convert Hex To ID Key Yamaha";
		this.unlockPasswordXdfToolStripMenuItem.Name = "unlockPasswordXdfToolStripMenuItem";
		this.unlockPasswordXdfToolStripMenuItem.Size = new System.Drawing.Size(266, 22);
		this.unlockPasswordXdfToolStripMenuItem.Text = "\ud83d\udd13 Unlock Password Xdf";
		this.toolStripMenuItem23.Name = "toolStripMenuItem23";
		this.toolStripMenuItem23.Size = new System.Drawing.Size(55, 21);
		this.toolStripMenuItem23.Text = "➤➤➤";
		this.breakpointToolStripMenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.breakpoinRPMToolStripMenuItem, this.breakpointTPSToolStripMenuItem, this.breakpointIAPToolStripMenuItem });
		this.breakpointToolStripMenuItem2.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.breakpointToolStripMenuItem2.Name = "breakpointToolStripMenuItem2";
		this.breakpointToolStripMenuItem2.Size = new System.Drawing.Size(99, 21);
		this.breakpointToolStripMenuItem2.Text = "\ud83d\uded1Breakpoint";
		this.breakpoinRPMToolStripMenuItem.Name = "breakpoinRPMToolStripMenuItem";
		this.breakpoinRPMToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
		this.breakpoinRPMToolStripMenuItem.Text = "RPM";
		this.breakpoinRPMToolStripMenuItem.Click += new System.EventHandler(this.breakpoinRPMToolStripMenuItem_Click_1);
		this.breakpointTPSToolStripMenuItem.Name = "breakpointTPSToolStripMenuItem";
		this.breakpointTPSToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
		this.breakpointTPSToolStripMenuItem.Text = "TPS";
		this.breakpointTPSToolStripMenuItem.Click += new System.EventHandler(this.breakpointTPSToolStripMenuItem_Click_1);
		this.breakpointIAPToolStripMenuItem.Name = "breakpointIAPToolStripMenuItem";
		this.breakpointIAPToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
		this.breakpointIAPToolStripMenuItem.Text = "IAP";
		this.breakpointIAPToolStripMenuItem.Click += new System.EventHandler(this.breakpointIAPToolStripMenuItem_Click_1);
		this.fueToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.rpmVsTps1ToolStripMenuItem, this.rpmVsTps2ToolStripMenuItem, this.rpmVsToolStripMenuItem, this.rpmVsIAPToolStripMenuItem });
		this.fueToolStripMenuItem.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.fueToolStripMenuItem.Name = "fueToolStripMenuItem";
		this.fueToolStripMenuItem.Size = new System.Drawing.Size(58, 21);
		this.fueToolStripMenuItem.Text = "⛽Fuel";
		this.rpmVsTps1ToolStripMenuItem.Name = "rpmVsTps1ToolStripMenuItem";
		this.rpmVsTps1ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.rpmVsTps1ToolStripMenuItem.Text = "RPM vs TPS 1";
		this.rpmVsTps1ToolStripMenuItem.Click += new System.EventHandler(this.rpmVsTps1ToolStripMenuItem_Click);
		this.rpmVsTps2ToolStripMenuItem.Name = "rpmVsTps2ToolStripMenuItem";
		this.rpmVsTps2ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.rpmVsTps2ToolStripMenuItem.Text = "RPM vs TPS 2";
		this.rpmVsTps2ToolStripMenuItem.Click += new System.EventHandler(this.rpmVsTps2ToolStripMenuItem_Click);
		this.rpmVsToolStripMenuItem.Name = "rpmVsToolStripMenuItem";
		this.rpmVsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.rpmVsToolStripMenuItem.Text = "RPM vs TPS 3";
		this.rpmVsToolStripMenuItem.Click += new System.EventHandler(this.rpmVsToolStripMenuItem_Click);
		this.rpmVsIAPToolStripMenuItem.Name = "rpmVsIAPToolStripMenuItem";
		this.rpmVsIAPToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
		this.rpmVsIAPToolStripMenuItem.Text = "RPM vs IAP";
		this.rpmVsIAPToolStripMenuItem.Click += new System.EventHandler(this.rpmVsIAPToolStripMenuItem_Click);
		this.ignitionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.toolStripMenuItem6, this.toolStripMenuItem7, this.toolStripMenuItem8, this.ignition4ToolStripMenuItem, this.ignition5ToolStripMenuItem });
		this.ignitionToolStripMenuItem.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.ignitionToolStripMenuItem.Name = "ignitionToolStripMenuItem";
		this.ignitionToolStripMenuItem.Size = new System.Drawing.Size(78, 21);
		this.ignitionToolStripMenuItem.Text = "⚡Ignition";
		this.toolStripMenuItem6.Name = "toolStripMenuItem6";
		this.toolStripMenuItem6.Size = new System.Drawing.Size(128, 22);
		this.toolStripMenuItem6.Text = "Ignition 1";
		this.toolStripMenuItem6.Click += new System.EventHandler(this.toolStripMenuItem6_Click);
		this.toolStripMenuItem7.Name = "toolStripMenuItem7";
		this.toolStripMenuItem7.Size = new System.Drawing.Size(128, 22);
		this.toolStripMenuItem7.Text = "Ignition 2";
		this.toolStripMenuItem7.Click += new System.EventHandler(this.toolStripMenuItem7_Click);
		this.toolStripMenuItem8.Name = "toolStripMenuItem8";
		this.toolStripMenuItem8.Size = new System.Drawing.Size(128, 22);
		this.toolStripMenuItem8.Text = "Ignition 3";
		this.toolStripMenuItem8.Click += new System.EventHandler(this.toolStripMenuItem8_Click);
		this.ignition4ToolStripMenuItem.Name = "ignition4ToolStripMenuItem";
		this.ignition4ToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
		this.ignition4ToolStripMenuItem.Text = "Ignition 4";
		this.ignition4ToolStripMenuItem.Click += new System.EventHandler(this.ignition4ToolStripMenuItem_Click);
		this.ignition5ToolStripMenuItem.Name = "ignition5ToolStripMenuItem";
		this.ignition5ToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
		this.ignition5ToolStripMenuItem.Text = "Ignition 5";
		this.ignition5ToolStripMenuItem.Click += new System.EventHandler(this.ignition5ToolStripMenuItem_Click);
		this.limiterToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[7] { this.toolStripMenuItem10, this.toolStripMenuItem11, this.toolStripMenuItem12, this.toolStripMenuItem13, this.toolStripMenuItem14, this.limiter6ToolStripMenuItem, this.limiter7ToolStripMenuItem });
		this.limiterToolStripMenuItem.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.limiterToolStripMenuItem.Name = "limiterToolStripMenuItem";
		this.limiterToolStripMenuItem.Size = new System.Drawing.Size(75, 21);
		this.limiterToolStripMenuItem.Text = "\ud83d\udcc9Limiter";
		this.toolStripMenuItem10.Name = "toolStripMenuItem10";
		this.toolStripMenuItem10.Size = new System.Drawing.Size(125, 22);
		this.toolStripMenuItem10.Text = "Limiter 1";
		this.toolStripMenuItem10.Click += new System.EventHandler(this.toolStripMenuItem10_Click);
		this.toolStripMenuItem11.Name = "toolStripMenuItem11";
		this.toolStripMenuItem11.Size = new System.Drawing.Size(125, 22);
		this.toolStripMenuItem11.Text = "Limiter 2";
		this.toolStripMenuItem11.Click += new System.EventHandler(this.toolStripMenuItem11_Click);
		this.toolStripMenuItem12.Name = "toolStripMenuItem12";
		this.toolStripMenuItem12.Size = new System.Drawing.Size(125, 22);
		this.toolStripMenuItem12.Text = "Limiter 3";
		this.toolStripMenuItem12.Click += new System.EventHandler(this.toolStripMenuItem12_Click);
		this.toolStripMenuItem13.Name = "toolStripMenuItem13";
		this.toolStripMenuItem13.Size = new System.Drawing.Size(125, 22);
		this.toolStripMenuItem13.Text = "Limiter 4";
		this.toolStripMenuItem13.Click += new System.EventHandler(this.toolStripMenuItem13_Click);
		this.toolStripMenuItem14.Name = "toolStripMenuItem14";
		this.toolStripMenuItem14.Size = new System.Drawing.Size(125, 22);
		this.toolStripMenuItem14.Text = "Limiter 5";
		this.toolStripMenuItem14.Click += new System.EventHandler(this.toolStripMenuItem14_Click);
		this.limiter6ToolStripMenuItem.Name = "limiter6ToolStripMenuItem";
		this.limiter6ToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
		this.limiter6ToolStripMenuItem.Text = "Limiter 6";
		this.limiter6ToolStripMenuItem.Click += new System.EventHandler(this.limiter6ToolStripMenuItem_Click);
		this.limiter7ToolStripMenuItem.Name = "limiter7ToolStripMenuItem";
		this.limiter7ToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
		this.limiter7ToolStripMenuItem.Text = "Limiter 7";
		this.limiter7ToolStripMenuItem.Click += new System.EventHandler(this.limiter7ToolStripMenuItem_Click);
		this.settinganLaiinToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[14]
		{
			this.toolStripMenuItem16, this.toolStripMenuItem17, this.toolStripMenuItem18, this.toolStripMenuItem19, this.toolStripMenuItem20, this.toolStripMenuItem21, this.toolStripMenuItem22, this.offMode4ToolStripMenuItem, this.offBASSensorToolStripMenuItem, this.offMAPSensorToolStripMenuItem,
			this.offECTSensorToolStripMenuItem, this.offIATSensorToolStripMenuItem, this.offVSSSensorToolStripMenuItem, this.offMAPSensorToolStripMenuItem1
		});
		this.settinganLaiinToolStripMenuItem.Font = new System.Drawing.Font("Franklin Gothic Medium", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.settinganLaiinToolStripMenuItem.Name = "settinganLaiinToolStripMenuItem";
		this.settinganLaiinToolStripMenuItem.Size = new System.Drawing.Size(117, 21);
		this.settinganLaiinToolStripMenuItem.Text = "⚙\ufe0fSettingan Lain";
		this.toolStripMenuItem16.Name = "toolStripMenuItem16";
		this.toolStripMenuItem16.Size = new System.Drawing.Size(192, 22);
		this.toolStripMenuItem16.Text = "OFF IMMO";
		this.toolStripMenuItem16.Click += new System.EventHandler(this.toolStripMenuItem16_Click);
		this.toolStripMenuItem17.Name = "toolStripMenuItem17";
		this.toolStripMenuItem17.Size = new System.Drawing.Size(192, 22);
		this.toolStripMenuItem17.Text = "OFF DTC O2";
		this.toolStripMenuItem17.Click += new System.EventHandler(this.toolStripMenuItem17_Click);
		this.toolStripMenuItem18.Name = "toolStripMenuItem18";
		this.toolStripMenuItem18.Size = new System.Drawing.Size(192, 22);
		this.toolStripMenuItem18.Text = "OFF O2 CORRECTION";
		this.toolStripMenuItem18.Click += new System.EventHandler(this.toolStripMenuItem18_Click);
		this.toolStripMenuItem19.Name = "toolStripMenuItem19";
		this.toolStripMenuItem19.Size = new System.Drawing.Size(192, 22);
		this.toolStripMenuItem19.Text = "OFF MODE 0";
		this.toolStripMenuItem19.Click += new System.EventHandler(this.toolStripMenuItem19_Click);
		this.toolStripMenuItem20.Name = "toolStripMenuItem20";
		this.toolStripMenuItem20.Size = new System.Drawing.Size(192, 22);
		this.toolStripMenuItem20.Text = "OFF MODE 1";
		this.toolStripMenuItem20.Click += new System.EventHandler(this.toolStripMenuItem20_Click);
		this.toolStripMenuItem21.Name = "toolStripMenuItem21";
		this.toolStripMenuItem21.Size = new System.Drawing.Size(192, 22);
		this.toolStripMenuItem21.Text = "OFF MODE 2";
		this.toolStripMenuItem21.Click += new System.EventHandler(this.toolStripMenuItem21_Click);
		this.toolStripMenuItem22.Name = "toolStripMenuItem22";
		this.toolStripMenuItem22.Size = new System.Drawing.Size(192, 22);
		this.toolStripMenuItem22.Text = "OFF MODE 3";
		this.toolStripMenuItem22.Click += new System.EventHandler(this.toolStripMenuItem22_Click);
		this.offMode4ToolStripMenuItem.Name = "offMode4ToolStripMenuItem";
		this.offMode4ToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
		this.offMode4ToolStripMenuItem.Text = "OFF MODE 4";
		this.offMode4ToolStripMenuItem.Click += new System.EventHandler(this.offMode4ToolStripMenuItem_Click);
		this.offBASSensorToolStripMenuItem.Name = "offBASSensorToolStripMenuItem";
		this.offBASSensorToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
		this.offBASSensorToolStripMenuItem.Text = "OFF BAS SENSOR";
		this.offBASSensorToolStripMenuItem.Click += new System.EventHandler(this.offBASSensorToolStripMenuItem_Click);
		this.offMAPSensorToolStripMenuItem.Name = "offMAPSensorToolStripMenuItem";
		this.offMAPSensorToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
		this.offMAPSensorToolStripMenuItem.Text = "OFF IACV SENSOR";
		this.offMAPSensorToolStripMenuItem.Click += new System.EventHandler(this.offMAPSensorToolStripMenuItem_Click);
		this.offECTSensorToolStripMenuItem.Name = "offECTSensorToolStripMenuItem";
		this.offECTSensorToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
		this.offECTSensorToolStripMenuItem.Text = "OFF ECT SENSOR";
		this.offECTSensorToolStripMenuItem.Click += new System.EventHandler(this.offECTSensorToolStripMenuItem_Click);
		this.offIATSensorToolStripMenuItem.Name = "offIATSensorToolStripMenuItem";
		this.offIATSensorToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
		this.offIATSensorToolStripMenuItem.Text = "OFF IAT SENSOR";
		this.offIATSensorToolStripMenuItem.Click += new System.EventHandler(this.offIATSensorToolStripMenuItem_Click);
		this.offVSSSensorToolStripMenuItem.Name = "offVSSSensorToolStripMenuItem";
		this.offVSSSensorToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
		this.offVSSSensorToolStripMenuItem.Text = "OFF VSS SENSOR";
		this.offVSSSensorToolStripMenuItem.Click += new System.EventHandler(this.offVSSSensorToolStripMenuItem_Click);
		this.offMAPSensorToolStripMenuItem1.Name = "offMAPSensorToolStripMenuItem1";
		this.offMAPSensorToolStripMenuItem1.Size = new System.Drawing.Size(192, 22);
		this.offMAPSensorToolStripMenuItem1.Text = "OFF MAP SENSOR";
		this.offMAPSensorToolStripMenuItem1.Click += new System.EventHandler(this.offMAPSensorToolStripMenuItem1_Click);
		this.comboBox2.BackColor = System.Drawing.Color.Black;
		this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBox2.Font = new System.Drawing.Font("Segoe UI Semibold", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.comboBox2.ForeColor = System.Drawing.Color.Cyan;
		this.comboBox2.FormattingEnabled = true;
		this.comboBox2.Location = new System.Drawing.Point(67, 9);
		this.comboBox2.Name = "comboBox2";
		this.comboBox2.Size = new System.Drawing.Size(78, 23);
		this.comboBox2.TabIndex = 17;
		this.comboBox2.SelectedIndexChanged += new System.EventHandler(this.comboBox2_SelectedIndexChanged);
		this.groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.groupBox1.Controls.Add(this.label7);
		this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.groupBox1.ForeColor = System.Drawing.Color.Cyan;
		this.groupBox1.Location = new System.Drawing.Point(314, 27);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(148, 81);
		this.groupBox1.TabIndex = 18;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "RPM";
		this.groupBox2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.groupBox2.Controls.Add(this.label6);
		this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.groupBox2.ForeColor = System.Drawing.Color.Cyan;
		this.groupBox2.Location = new System.Drawing.Point(475, 27);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(148, 81);
		this.groupBox2.TabIndex = 19;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "TPS";
		this.groupBox3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.groupBox3.Controls.Add(this.label8);
		this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.groupBox3.ForeColor = System.Drawing.Color.Cyan;
		this.groupBox3.Location = new System.Drawing.Point(637, 27);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Size = new System.Drawing.Size(148, 81);
		this.groupBox3.TabIndex = 19;
		this.groupBox3.TabStop = false;
		this.groupBox3.Text = "INJECTOR";
		this.groupBox4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.groupBox4.Controls.Add(this.label5);
		this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.groupBox4.ForeColor = System.Drawing.Color.Cyan;
		this.groupBox4.Location = new System.Drawing.Point(799, 27);
		this.groupBox4.Name = "groupBox4";
		this.groupBox4.Size = new System.Drawing.Size(148, 81);
		this.groupBox4.TabIndex = 19;
		this.groupBox4.TabStop = false;
		this.groupBox4.Text = "IGNITION";
		this.groupBox5.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.groupBox5.Controls.Add(this.label4);
		this.groupBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.groupBox5.ForeColor = System.Drawing.Color.Cyan;
		this.groupBox5.Location = new System.Drawing.Point(961, 27);
		this.groupBox5.Name = "groupBox5";
		this.groupBox5.Size = new System.Drawing.Size(148, 81);
		this.groupBox5.TabIndex = 19;
		this.groupBox5.TabStop = false;
		this.groupBox5.Text = "SUHU MESIN";
		this.groupBox6.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.groupBox6.Controls.Add(this.label11);
		this.groupBox6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.groupBox6.ForeColor = System.Drawing.Color.Cyan;
		this.groupBox6.Location = new System.Drawing.Point(1123, 27);
		this.groupBox6.Name = "groupBox6";
		this.groupBox6.Size = new System.Drawing.Size(148, 81);
		this.groupBox6.TabIndex = 20;
		this.groupBox6.TabStop = false;
		this.groupBox6.Text = "HP";
		this.label15.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label15.AutoSize = true;
		this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label15.ForeColor = System.Drawing.Color.Cyan;
		this.label15.Location = new System.Drawing.Point(942, 126);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(66, 15);
		this.label15.TabIndex = 21;
		this.label15.Text = "Start Offset";
		this.label16.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label16.AutoSize = true;
		this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label16.ForeColor = System.Drawing.Color.Cyan;
		this.label16.Location = new System.Drawing.Point(1103, 127);
		this.label16.Name = "label16";
		this.label16.Size = new System.Drawing.Size(86, 15);
		this.label16.TabIndex = 22;
		this.label16.Text = "Ceksum Offset";
		this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label1.ForeColor = System.Drawing.Color.Cyan;
		this.label1.Location = new System.Drawing.Point(4, 127);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(42, 19);
		this.label1.TabIndex = 1;
		this.label1.Text = "File";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label1.Click += new System.EventHandler(this.label1_Click);
		this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
		this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label19.ForeColor = System.Drawing.Color.Cyan;
		this.label19.Location = new System.Drawing.Point(6, 12);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(55, 19);
		this.label19.TabIndex = 23;
		this.label19.Text = "Function :";
		this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label20.ForeColor = System.Drawing.Color.Cyan;
		this.label20.Location = new System.Drawing.Point(145, 13);
		this.label20.Name = "label20";
		this.label20.Size = new System.Drawing.Size(34, 19);
		this.label20.TabIndex = 24;
		this.label20.Text = "Value :";
		this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupBox7.Controls.Add(this.button2);
		this.groupBox7.Controls.Add(this.label19);
		this.groupBox7.Controls.Add(this.label20);
		this.groupBox7.Controls.Add(this.txtInput);
		this.groupBox7.Controls.Add(this.comboBox2);
		this.groupBox7.Location = new System.Drawing.Point(166, 114);
		this.groupBox7.Name = "groupBox7";
		this.groupBox7.Size = new System.Drawing.Size(290, 37);
		this.groupBox7.TabIndex = 20;
		this.groupBox7.TabStop = false;
		this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
		this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.pictureBox1.Image = (System.Drawing.Image)resources.GetObject("pictureBox1.Image");
		this.pictureBox1.Location = new System.Drawing.Point(1109, 637);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(162, 37);
		this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
		this.pictureBox1.TabIndex = 20;
		this.pictureBox1.TabStop = false;
		this.label2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label2.BackColor = System.Drawing.Color.Yellow;
		this.label2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.label2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label2.Location = new System.Drawing.Point(1309, 148);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(98, 44);
		this.label2.TabIndex = 16;
		this.label2.Text = "AFR";
		this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label9.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label9.BackColor = System.Drawing.Color.Yellow;
		this.label9.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.label9.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label9.Location = new System.Drawing.Point(1290, 180);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(98, 41);
		this.label9.TabIndex = 18;
		this.label9.Text = "Idc";
		this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label10.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.label10.BackColor = System.Drawing.Color.Yellow;
		this.label10.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.label10.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label10.Location = new System.Drawing.Point(1271, 159);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(93, 44);
		this.label10.TabIndex = 17;
		this.label10.Text = "Load %";
		this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.button4.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.button4.Location = new System.Drawing.Point(1341, 369);
		this.button4.Name = "button4";
		this.button4.Size = new System.Drawing.Size(81, 23);
		this.button4.TabIndex = 13;
		this.button4.Text = "Undo";
		this.button4.UseVisualStyleBackColor = true;
		this.button4.Click += new System.EventHandler(this.button4_Click_1);
		this.button6.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.button6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.button6.Location = new System.Drawing.Point(1451, 369);
		this.button6.Name = "button6";
		this.button6.Size = new System.Drawing.Size(76, 23);
		this.button6.TabIndex = 14;
		this.button6.Text = "Redo";
		this.button6.UseVisualStyleBackColor = true;
		this.button6.Click += new System.EventHandler(this.button6_Click);
		this.textBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.textBox1.Location = new System.Drawing.Point(1342, 343);
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(61, 20);
		this.textBox1.TabIndex = 16;
		this.textBox1.Text = "BORE";
		this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.textBox2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.textBox2.Location = new System.Drawing.Point(1428, 343);
		this.textBox2.Name = "textBox2";
		this.textBox2.Size = new System.Drawing.Size(61, 20);
		this.textBox2.TabIndex = 17;
		this.textBox2.Text = "STROKE";
		this.textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.label12.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.label12.BackColor = System.Drawing.Color.White;
		this.label12.Location = new System.Drawing.Point(1409, 343);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(13, 20);
		this.label12.TabIndex = 18;
		this.label12.Text = "X";
		this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label13.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.label13.BackColor = System.Drawing.Color.Silver;
		this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label13.ForeColor = System.Drawing.Color.Black;
		this.label13.Location = new System.Drawing.Point(1342, 316);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(185, 23);
		this.label13.TabIndex = 3;
		this.label13.Text = "Volume Silinder (CC)";
		this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.button10.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.button10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, 0);
		this.button10.Location = new System.Drawing.Point(1472, 342);
		this.button10.Name = "button10";
		this.button10.Size = new System.Drawing.Size(31, 20);
		this.button10.TabIndex = 19;
		this.button10.Text = "Ok";
		this.button10.UseVisualStyleBackColor = true;
		this.button10.Click += new System.EventHandler(this.button10_Click);
		this.panelDATA.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.panelDATA.BackColor = System.Drawing.Color.Black;
		this.panelDATA.BackgroundImage = (System.Drawing.Image)resources.GetObject("panelDATA.BackgroundImage");
		this.panelDATA.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.panelDATA.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panelDATA.Controls.Add(this.button10);
		this.panelDATA.Controls.Add(this.label13);
		this.panelDATA.Controls.Add(this.label12);
		this.panelDATA.Controls.Add(this.textBox2);
		this.panelDATA.Controls.Add(this.textBox1);
		this.panelDATA.Controls.Add(this.button6);
		this.panelDATA.Controls.Add(this.button4);
		this.panelDATA.Controls.Add(this.label10);
		this.panelDATA.Controls.Add(this.label9);
		this.panelDATA.Controls.Add(this.label2);
		this.panelDATA.Font = new System.Drawing.Font("Lucida Console", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.panelDATA.ForeColor = System.Drawing.Color.Black;
		this.panelDATA.Location = new System.Drawing.Point(8, 157);
		this.panelDATA.Name = "panelDATA";
		this.panelDATA.Size = new System.Drawing.Size(1263, 476);
		this.panelDATA.TabIndex = 3;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Black;
		base.ClientSize = new System.Drawing.Size(1280, 683);
		base.Controls.Add(this.groupBox7);
		base.Controls.Add(this.button3);
		base.Controls.Add(this.pictureBox1);
		base.Controls.Add(this.label16);
		base.Controls.Add(this.CbFastWrite);
		base.Controls.Add(this.label15);
		base.Controls.Add(this.button12);
		base.Controls.Add(this.groupBox6);
		base.Controls.Add(this.groupBox2);
		base.Controls.Add(this.comboBox1);
		base.Controls.Add(this.groupBox5);
		base.Controls.Add(this.groupBox4);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.TbStartOffset);
		base.Controls.Add(this.TbFileName);
		base.Controls.Add(this.button8);
		base.Controls.Add(this.groupBox3);
		base.Controls.Add(this.groupBox1);
		base.Controls.Add(this.TbChecksumOffset);
		base.Controls.Add(this.panelDATA);
		base.Controls.Add(this.panel4);
		base.Controls.Add(this.menuStrip7);
		base.Controls.Add(this.show_all);
		base.Controls.Add(this.BtnWrite);
		base.Controls.Add(this.button1);
		base.Controls.Add(this.TxtPartCode);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = true;
		base.MinimumSize = new System.Drawing.Size(1120, 640);
		base.Name = "Form1";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "H2 MOTOR ECU TUNEX";
		base.Load += new System.EventHandler(this.Form1_Load);
		this.panel4.ResumeLayout(false);
		this.menuStrip7.ResumeLayout(false);
		this.menuStrip7.PerformLayout();
		this.groupBox1.ResumeLayout(false);
		this.groupBox2.ResumeLayout(false);
		this.groupBox3.ResumeLayout(false);
		this.groupBox4.ResumeLayout(false);
		this.groupBox5.ResumeLayout(false);
		this.groupBox6.ResumeLayout(false);
		this.groupBox7.ResumeLayout(false);
		this.groupBox7.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		this.panelDATA.ResumeLayout(false);
		this.panelDATA.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
