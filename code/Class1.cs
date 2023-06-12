using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

//�Q��/�A�Z���u��/System.Windows.Forms.dll
//�N���b�v�{�[�h����̏����B
using System.Windows.Forms;

//match
using System.Text.RegularExpressions;
using CeVIO.Talk.RemoteService2; 

///unsafe�R���p�C���I�v�V����
namespace CShar {
    public class Class1 {
        static Talker2 talker = new Talker2(); 
        static string PATH ;
        static string CharConfig ;
        static string GhostConfig;

        static string OldSection = "";
        static string Voice1     = "";
        static string Voice2     = "";
        static string Voice3     = "";

        static int TaskLen       = 0;
        static int BreakFlag     = 0;
        static int CheckLoop     = 0;

        static void Main( string[] args ) {
            Console.WriteLine( "test" );
        }

        [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileInt")] 
        static extern uint GetPrivateProfileInt( string String, string Key, int Default, string FilePath); 

        [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileString")] 
        static extern uint GetPrivateProfileString(string Section, string Key, string Default, StringBuilder Value, uint ValueSize, string FilePath ); 


        [DllExport] 
        public static unsafe bool load( IntPtr h , int len ) {
            Console.WriteLine( "CeVIO-Talker V2 Load" );
            PATH = Marshal.PtrToStringAnsi( h , len );
            CharConfig  = PATH + "Char.ini";
            GhostConfig = PATH + "Ghost.ini";

            CeVIO_Start().Wait();
            Marshal.FreeHGlobal(h);
            return true;
        }
        [DllExport] 
        public static unsafe bool unload() {
            CeVIO_End();
            Console.WriteLine( "CeVIO-Talker V2 Unload" );
            return true;
        }

        [DllExport] 
        //C#����int�^���B
        //������null�������܂܂Ȃ��������B
        public static unsafe IntPtr request( IntPtr h, IntPtr len){
            //Console.OutputEncoding = Encoding.UTF8;
            //Console.WriteLine( Console.OutputEncoding.CodePage );
            //Console.WriteLine( Console.InputEncoding.CodePage  );
            string ID                 = "";
            string Reference0         = "";
            string Reference1         = "";
            //Reference4
            string SakuraScript       = "";

            //�Ԃ�l�B�󋵂ɂ���ď㏑���B
            string resString = "PLUGIN/2.0 204 No Content\r\n\r\n";

            //������
            int req_len = Marshal.ReadInt32( len );
            //����
            string req = Marshal.PtrToStringAnsi( h , req_len );
            Marshal.FreeHGlobal(h);
            //Console.WriteLine( req );
            
            string[] sep = {"\r\n"};
            string[] requestText = req.Split( sep , StringSplitOptions.None );
            foreach ( string requestLine in requestText ) {
                //Console.WriteLine( "check : " + requestLine );

                if ( Regex.IsMatch( requestLine , "ID: .+") ) {
                    string[] lineSep = {"ID: "};
                    string[] lineValue = requestLine.Split( lineSep , StringSplitOptions.None );
                    ID = lineValue[1] ;
                }

                if ( Regex.IsMatch( requestLine , "Reference0: .+") ) {
                    string[] lineSep = {"Reference0: "};
                    string[] lineValue = requestLine.Split( lineSep , StringSplitOptions.None );
                    Reference0 = lineValue[1] ;
                }

                if ( Regex.IsMatch( requestLine , "Reference1: .+") ) {
                    string[] lineSep = {"Reference1: "};
                    string[] lineValue = requestLine.Split( lineSep , StringSplitOptions.None );
                    Reference1 = lineValue[1] ;
                }

                if ( Regex.IsMatch( requestLine , "Reference4: .+") ) {
                    string[] lineSep = {"Reference4: "};
                    string[] lineValue = requestLine.Split( lineSep , StringSplitOptions.None );
                    SakuraScript = lineValue[1] ;
                }
            }

            //ID check
            if ( ID == "version" ){
                //SSP�Ƃ̒ʐM�̕����R�[�h���m�肳����B
                //�R���\�[���ɑ΂��āA���̊�����Shift_JIS����Ȃ��Ɖ�����B
                resString = "PLUGIN/2.0 200 OK\r\nSender: CShar\r\nCharset: Shift_JIS\r\nValue: 1.0.0\r\n\r\n";

            } else if ( ID == "OnMenuExec" ) {
                resString = "PLUGIN/2.0 200 OK\r\nSender: CShar\r\nCharset: Shift_JIS\r\nValue: �S�[�X�g�����N���b�v�{�[�h�ɃR�s�[���܂����B\r\n\r\n";
                //���j���[���Ăяo�����S�[�X�g�̃��j���[��
                Clipboard.SetText( Reference1 );

            } else if ( ID == "OnOtherGhostTalk" ) {
                //�X���b�h�œK���ɁB
                CeVIO_Task( SakuraScript , Reference0 );
            }

            //realloc �T�C�Y�̍Ċ��蓖��
            //utf-8���ߑł�
            //null�����͍l������K�v�͂Ȃ��������AStringToHGlobalAnsi��null�������܂ށB
            int str_len = resString.Length;
            Marshal.WriteInt32( len , 3 * str_len + 1 );
            IntPtr res = Marshal.StringToHGlobalAnsi( resString );
            return res;
        }
        public static Task CeVIO_Start() {
            return Task.Run(() => {
                    //false����Ȃ��ƏI�����Ɉꏏ�ɏI���ł��Ȃ��B
                    ServiceControl2.StartHost(false); 
                    Console.WriteLine( "Start CeVIO" );
                    });
        }
        //SSP�̏I���ɒu���čs����邱�Ƃ�����B
        public static void CeVIO_End() {
            bool CheckStartedCeVIO = ServiceControl2.IsHostStarted; 
            int CheckStartedCeVIO_Count = 0;

            if( talker.Cast != null ){
                BreakFlag = 1;
                talker.Stop();
            }

            while ( CheckStartedCeVIO == false ) {
                //���ɏI�����Ă����ꍇ���l�����čő吔��p�ӂ��Ă����B
                if ( CheckStartedCeVIO_Count >= 8 ) { break; }
                CheckStartedCeVIO_Count++;
                Thread.Sleep( 500 );
                CheckStartedCeVIO = ServiceControl2.IsHostStarted; 
            }
            ServiceControl2.CloseHost(); 
            Console.WriteLine( "Close CeVIO" );
        }

        public static void CeVIO_Talk( string SakuraScript , string GhostName ) {

            //��������Ghost���ƈႤ�ꍇ�`�F�b�N
            if( OldSection != GhostName ){
                //�Z�N�V���������݂��Ă��AFlag = 1
                string ChangeSection = "Default";
                uint exsistFlag = GetPrivateProfileInt( GhostName , "Flag", 0 , GhostConfig ); 
                if( exsistFlag == 1 ){
                    ChangeSection = GhostName;
                }

                int StringSize = 32; 
                StringBuilder sb = new StringBuilder(StringSize); 
                GetPrivateProfileString( ChangeSection , "Char1", "", sb, Convert.ToUInt32(sb.Capacity), GhostConfig ); 
                Voice1 = sb.ToString();

                sb = new StringBuilder(StringSize); 
                GetPrivateProfileString( ChangeSection , "Char2", "", sb, Convert.ToUInt32(sb.Capacity), GhostConfig ); 
                Voice2 = sb.ToString();

                sb = new StringBuilder(StringSize); 
                GetPrivateProfileString( ChangeSection , "Char3", "", sb, Convert.ToUInt32(sb.Capacity), GhostConfig ); 
                Voice3 = sb.ToString();

                OldSection = ChangeSection;
            }

            string[] SakuraScriptSep = {"�B"};
            string[] SakuraScripts = SakuraScript.Split( SakuraScriptSep , StringSplitOptions.None );
            foreach ( string line in SakuraScripts ) {
                CheckLoop = 1;

                //�������̃S�[�X�g����ۑ�����
                //char1,2,3�����ꂼ��static�ɁB
                //�S�[�X�g���������Ƃ��擾���X���[���ď��������炷�B
                string talkText = Regex.Replace( line  , "\\\\[01p]" , "" );
                if( talkText == "" ){
                    continue;
                }

                Match m;
                m = Regex.Match( line  , "^\\\\0" );
                if( m.Success ){
                    if ( Voice1 == "" ){ continue; } 
                    TalkSetting( Voice1 );
                }

                m = Regex.Match( line  , "^\\\\1" );
                if( m.Success ){
                    if ( Voice2 == "" ){ continue; } 
                    TalkSetting( Voice2 );
                }

                m = Regex.Match( line  , "^\\\\p" );
                if( m.Success ){
                    if ( Voice3 == "" ){ continue; } 
                    TalkSetting( Voice3 );
                }
                //������}�b�`�Ȃ����ƑO��g�p�������̂��g�p�����B
                //���̃S�[�X�g�ƍ��킹��Default���g�p����ꍇ�������Ă��܂��ꍇ������B
                //�P�̂̃S�[�X�g�Ō���Ƃ��̋����͈��肵�Ă���B

                //Console.WriteLine( "cast    " + talker.Cast );
                //Console.WriteLine( "line    " + talkText );
                //Console.WriteLine( "Ghost   " + GhostName );
                //Console.WriteLine( "section " + OldSection );
                
                //\0\1�����Ȃɂ��w�肳��Ă��Ȃ���ԂŔ�������e�L�X�g���J�b�g����B
                if( talker.Cast == null ){
                    continue;
                }
                
                SpeakingState2 state = talker.Speak( talkText );
                //SpeakingState2 state = talker.Speak( "��D��" ); 
                //�I�����Ȃ����炢��Ȃ��B
                //loop���͎g���B
                state.Wait(); 
                if( BreakFlag == 1 ){
                    break;
                };
            }
            CheckLoop = 0;
            BreakFlag = 0;
        }

        public static Task CeVIO_Task( string SakuraScript , string GhostName ) {
            return Task.Run(() => {


                    //���ݏ������Ă��Ȃ����A��݂̂����s���̏�Ԃ̂ݒʂ��B
                    //�������y��������B
                    if( TaskLen <= 1 ){
                        TaskLen++;

                        SakuraScript = ReplaceTalkText( SakuraScript );
                        string CheckSakuraScript;
                        CheckSakuraScript = Regex.Replace( SakuraScript  , "\\\\." , "" );
                        CheckSakuraScript = CheckSakuraScript.Replace( "�B" , "" );

                        //�󂶂�Ȃ������B
                        //���\�ł���B
                        if( ServiceControl2.IsHostStarted == true && CheckSakuraScript != "" ) {
                            //����ȊO�����[�v����stop 
                            if( talker.Cast != null && CheckLoop == 1 ){
                                BreakFlag = 1;
                                talker.Stop();
                                //���s�̏I���������Ԃɍ��킸�ɃX���[���Ă��܂����B
                                Thread.Sleep( 200 );
                            }
                            //���݂̃^�X�N�̐����m�F�o����悤�ɂ��邩�B
                            //Ctrl�������Ȃ���z�C�[�����g����6�ʃX�^�b�N����B
                            //Console.WriteLine( TaskLen );
                            //Thread.Sleep( 100 );
                            if( TaskLen == 1 ){
                                CeVIO_Talk( SakuraScript , GhostName);
                            }
                        }
                        TaskLen--;
                    }
                });
        }


        public static string ReplaceTalkText( string SakuraScript ){
            //Console.WriteLine( "origin " + SakuraScript );
            string text  = SakuraScript;
            text         = text.Replace( "("  , "" );
            text         = text.Replace( ")"  , "" );
            text         = text.Replace( "�i" , "" );
            text         = text.Replace( "�j" , "" );
            text         = text.Replace( "�u" , "" );
            text         = text.Replace( "�v" , "" );
            text         = text.Replace( "�y" , "" );
            text         = text.Replace( "�z" , "" );
            text         = text.Replace( "�H" , "?" );
            text         = text.Replace( "�I" , "!" );
            text         = text.Replace( "�`" , "�[" );
            text         = text.Replace( "�@" , " " );
            text         = text.Replace( "\\h" , "\\0" );
            text         = text.Replace( "\\u" , "\\1" );
            text         = text.Replace( "\\0" , ",\\0" );
            text         = text.Replace( "\\1" , ",\\1" );
            //�O�l�ڊm��
            text         = Regex.Replace( text  , "\\\\p\\[.*?\\]"   , ",\\p" );

            //���s�u���B
            text         = Regex.Replace( text  , "\\\\n\\[half\\]" , "�B" );
            text         = Regex.Replace( text  , "\\\\n\\[[0-9].*?\\]" , "�B" );
            text         = Regex.Replace( text  , "\\\\n" , "�B" );

            text         = Regex.Replace( text  , "\\\\__[a-zA-Z]\\[.*?\\]" , "" );
            text         = Regex.Replace( text  , "\\\\_[a-zA-Z]\\[.*?\\]"  , "" );
            text         = Regex.Replace( text  , "\\\\_[a-zA-Z]"           , "" );
            text         = Regex.Replace( text  , "\\\\!\\[.*?\\]"          , "" );
            //���d�l��q0[...]
            text         = Regex.Replace( text  , "\\\\q[0-9]\\[.*?\\]"   , "" );
            text         = Regex.Replace( text  , "\\\\[a-zA-Z]\\[.*?\\]"   , "" );
            text         = Regex.Replace( text  , "\\\\w[0-9]"              , "" );
            text         = Regex.Replace( text  , " +"                      , " " );

            text         = Regex.Replace( text  , "\\\\[^01p]" , "" );

            text         = text.Replace( "," , "�B" );
            text         = Regex.Replace( text  , "^�B" , "" );
            text         = Regex.Replace( text  , "$" , "�B" );
            text         = Regex.Replace( text  , "�B+" , "�B" );
            //Console.WriteLine( "replaced " + text );
            return text;
        }
        public static void TalkSetting( string talkChar ){
            talker.Cast         = talkChar;
            talker.Volume       = GetPrivateProfileInt( talker.Cast , "Volume"   , 70, CharConfig ); 
            talker.Speed        = GetPrivateProfileInt( talker.Cast , "Speed"    , 47, CharConfig ); 
            talker.Tone         = GetPrivateProfileInt( talker.Cast , "Tone"     , 50, CharConfig ); 
            talker.Alpha        = GetPrivateProfileInt( talker.Cast , "Alpha"    , 50, CharConfig ); 
            talker.ToneScale    = GetPrivateProfileInt( talker.Cast , "ToneScale", 50, CharConfig ); 

            //�I�𒆂̃L�����̊���X�g���擾 
            TalkerComponentCollection2 CharStatus = talker.Components; 
            for( int i = 0 ; i < CharStatus.Count; i++ ){ 
                CharStatus[i].Value = GetPrivateProfileInt( talker.Cast , CharStatus[i].Name , 0 , CharConfig ); 
            } 
            //talker.Cast = "ONE";
            //talker.Volume       = 70; 
            //talker.Speed        = 47; 
            //talker.Tone         = 50; 
            //talker.Alpha        = 50; 
            //talker.ToneScale    = 50; 

            //CharStatus["Bright"].Value      = 0; 
            //CharStatus["Normal"].Value      = 100; 
            //CharStatus["Strong"].Value      = 0; 
            //CharStatus[ "Dark" ].Value      = 32; 
            //CharStatus["Bright"].Value      = GetPrivateProfileInt("ONE", "Bright"   , 0  , CharConfig ); 
            //CharStatus["Normal"].Value      = GetPrivateProfileInt("ONE", "Normal"   , 100, CharConfig ); 
            //CharStatus["Strong"].Value      = GetPrivateProfileInt("ONE", "Strong"   , 0  , CharConfig ); 
            //CharStatus["Dark"].Value        = GetPrivateProfileInt("ONE", "Dark"     , 32 , CharConfig ); 
        }
    }
}



