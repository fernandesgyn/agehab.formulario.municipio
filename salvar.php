<?php
declare(strict_types=1);
require __DIR__.'/src/bootstrap.php';
if($_SERVER['REQUEST_METHOD']!=='POST'){http_response_code(405);header('Allow: POST');exit('Método não permitido.');}
if(!verify_csrf()){http_response_code(419);exit('Sessão inválida ou expirada. Reabra o formulário e tente novamente.');}
[$errors,$answer]=validate_questionnaire($_POST,municipalities());
if($errors){
 $_SESSION['form_old']=$_POST;unset($_SESSION['form_old']['_csrf']);
 $_SESSION['form_errors']=$errors;go('index.php');
}
try {
 $fields=questionnaire_fields();
 $columns=array_merge(['CodigoIBGE'],array_map(static fn(string $f):string=>strtoupper($f),$fields),['ProgramaHabitacionalTipo','ProgramaProprioDetalhes']);
 $markers=array_merge([':municipio'],array_map(static fn(string $f):string=>':'.$f,$fields),[':programa',':detalhes']);
 $sql='INSERT INTO dbo.RespostasMunicipais ('.implode(', ',$columns).') OUTPUT INSERTED.Id VALUES ('.implode(', ',$markers).')';
 $params=[':municipio'=>$answer['municipio'],':programa'=>$answer['programa'],':detalhes'=>$answer['programa_detalhes']===''?null:$answer['programa_detalhes']];
 foreach($fields as $field)$params[':'.$field]=$answer[$field];
 $statement=database()->prepare($sql);
 $statement->execute($params);
 $id=(int)$statement->fetchColumn();
 if($id<=0)throw new RuntimeException('Não foi possível obter o protocolo de envio.');
 unset($_SESSION['form_old'],$_SESSION['form_errors']);
 $_SESSION['success_protocol']=$id;
 go('concluido.php');
} catch(PDOException $exception){
 $state=(string)$exception->getCode();
 if(in_array($state,['23000','23505'],true)||str_contains($exception->getMessage(),'2627')||str_contains($exception->getMessage(),'2601')){
  $msg='Este município já possui uma resposta registrada. Procure a AGEHAB caso precise corrigir os dados.';
 }else{
  error_log('Formulário AGEHAB erro SQL Server: '.$exception->getMessage());
  $msg='O banco de dados não concluiu a gravação. Tente novamente ou contate a equipe responsável.';
 }
 $_SESSION['form_old']=$_POST;unset($_SESSION['form_old']['_csrf']);
 $_SESSION['form_errors']=['geral'=>$msg];go('index.php');
} catch(Throwable $exception){
 error_log('Formulário AGEHAB: '.$exception->getMessage());
 $_SESSION['form_old']=$_POST;unset($_SESSION['form_old']['_csrf']);
 $_SESSION['form_errors']=['geral'=>'Não foi possível concluir o envio. Contate a equipe responsável.'];
 go('index.php');
}
