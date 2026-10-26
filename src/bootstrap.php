<?php
declare(strict_types=1);
require_once __DIR__.'/questionnaire.php';

function load_environment(string $path):void {
 if(!is_file($path)||!is_readable($path))return;
 foreach(file($path,FILE_IGNORE_NEW_LINES|FILE_SKIP_EMPTY_LINES)?:[] as $line){
  $line=trim($line);
  if($line===''||str_starts_with($line,'#')||!str_contains($line,'='))continue;
  [$key,$value]=array_map('trim',explode('=',$line,2));
  if(!preg_match('/^[A-Z][A-Z0-9_]*$/D',$key)||getenv($key)!==false)continue;
  if(strlen($value)>1&&(($value[0]==='"'&&str_ends_with($value,'"'))||($value[0]==="'"&&str_ends_with($value,"'"))))$value=substr($value,1,-1);
  putenv($key.'='.$value);
 }
}
load_environment(dirname(__DIR__).'/.env');
date_default_timezone_set((string)(getenv('APP_TIMEZONE')?:'America/Sao_Paulo'));
$https=!empty($_SERVER['HTTPS'])&&$_SERVER['HTTPS']!=='off';
if(PHP_SAPI!=='cli'&&session_status()!==PHP_SESSION_ACTIVE){
 session_set_cookie_params(['lifetime'=>0,'path'=>'/','secure'=>$https,'httponly'=>true,'samesite'=>'Lax']);
 session_start();
 header('X-Content-Type-Options: nosniff');
 header('Referrer-Policy: same-origin');
 header('Cache-Control: no-store');
}
function h(mixed $value):string {
 return htmlspecialchars((string)$value,ENT_QUOTES|ENT_SUBSTITUTE,'UTF-8');
}
function csrf():string {
 if(!isset($_SESSION['csrf']))$_SESSION['csrf']=bin2hex(random_bytes(32));
 return $_SESSION['csrf'];
}
function verify_csrf():bool {
 return is_string($_POST['_csrf']??null)&&hash_equals(csrf(),(string)$_POST['_csrf']);
}
function go(string $path):never {
 header('Location: '.$path,true,303);exit;
}
function municipalities():array {
 static $all=null;
 return $all??=require dirname(__DIR__).'/data/municipios.php';
}
function database():PDO {
 static $pdo=null;
 if($pdo!==null)return $pdo;
 if(!extension_loaded('pdo_sqlsrv'))throw new RuntimeException('Extensão pdo_sqlsrv não habilitada.');
 $host=(string)(getenv('SQLSERVER_HOST')?:'');
 $port=(int)(getenv('SQLSERVER_PORT')?:1433);
 $db=(string)(getenv('SQLSERVER_DATABASE')?:'');
 $user=(string)(getenv('SQLSERVER_USERNAME')?:'');
 $pass=(string)(getenv('SQLSERVER_PASSWORD')?:'');
 if(!$host||!$db||!$user||!$pass||$port<1||$port>65535)throw new RuntimeException('Configure a conexão SQL Server no .env.');
 $encrypt=filter_var(getenv('SQLSERVER_ENCRYPT')?:'false',FILTER_VALIDATE_BOOLEAN)?'yes':'no';
 $trust=filter_var(getenv('SQLSERVER_TRUST_SERVER_CERTIFICATE')?:'true',FILTER_VALIDATE_BOOLEAN)?'yes':'no';
 $dsn="sqlsrv:Server={$host},{$port};Database={$db};Encrypt={$encrypt};TrustServerCertificate={$trust}";
 return $pdo=new PDO($dsn,$user,$pass,[
  PDO::ATTR_ERRMODE=>PDO::ERRMODE_EXCEPTION,
  PDO::ATTR_DEFAULT_FETCH_MODE=>PDO::FETCH_ASSOC,
  PDO::ATTR_EMULATE_PREPARES=>false
 ]);
}
function require_admin():void {
 if(empty($_SESSION['admin_authenticated'])){http_response_code(403);exit('Acesso não autorizado.');}
}
