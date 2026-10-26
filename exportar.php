<?php
declare(strict_types=1);
require __DIR__.'/src/bootstrap.php';require_admin();
try{
 $rows=database()->query('SELECT R.*, M.Nome AS Municipio FROM dbo.RespostasMunicipais R JOIN dbo.Municipios M ON M.CodigoIBGE=R.CodigoIBGE ORDER BY M.Nome')->fetchAll();
}catch(Throwable $exception){
 error_log('Exportacao do formulario AGEHAB: '.$exception->getMessage());
 http_response_code(503);exit('Não foi possível exportar os dados.');
}
header('Content-Type: text/csv; charset=UTF-8');
header('Content-Disposition: attachment; filename="respostas-municipios-goianos.csv"');
header('X-Content-Type-Options: nosniff');
$output=fopen('php://output','wb');fwrite($output,"\xEF\xBB\xBF");
$cols=array_merge(['Id','CodigoIBGE','Municipio'],array_map('strtoupper',questionnaire_fields()),['ProgramaHabitacionalTipo','ProgramaProprioDetalhes','CriadoEm']);
fputcsv($output,$cols,';');
foreach($rows as $row){
 $result=[];
 foreach($cols as $col){
  $v=$row[$col]??'';
  if($v instanceof DateTimeInterface)$v=$v->format('Y-m-d H:i:s');
  $v=(string)$v;
  if(preg_match('/^[\s]*[=+\-@]/u',$v))$v="'".$v;
  $result[]=$v;
 }
 fputcsv($output,$result,';');
}
fclose($output);
