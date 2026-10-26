<?php
declare(strict_types=1);
$path=rawurldecode(parse_url($_SERVER['REQUEST_URI']??'/',PHP_URL_PATH)?:'/');
if(str_contains($path,"\0")||preg_match('#(^|/)(\.|data(?:/|$)|src(?:/|$)|database(?:/|$)|partials(?:/|$)|tests(?:/|$)|storage(?:/|$))#i',$path)||preg_match('/\.(?:env|sql|md|yml|yaml|lock|json)$/i',$path)){
 http_response_code(404);exit('Não encontrado.');
}
if($path==='/'){require __DIR__.'/index.php';return true;}
if(is_file(__DIR__.$path))return false;
http_response_code(404);exit('Não encontrado.');
