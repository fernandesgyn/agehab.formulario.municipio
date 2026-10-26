<?php
declare(strict_types=1);

function questionnaire_areas(): array {
 return [
  2=>'Cadastro e seleção das famílias',
  3=>'Visitas sociais e elaboração de relatório social',
  4=>'Trabalho Social (TS) — fase pré-ocupacional',
  5=>'Trabalho Social (TS) — fase pós-ocupacional',
 ];
}
function questionnaire_contracts(): array {
 return [
  'efetivos'=>'Efetivos / concursados',
  'terceirizados'=>'Terceirizados',
  'temporarios'=>'Contrato temporário',
  'comissionados'=>'Cargo em comissão',
 ];
}
function questionnaire_fields(): array {
 $result=[];
 foreach(array_keys(questionnaire_areas()) as $number) {
  foreach(array_keys(questionnaire_contracts()) as $contract) {
   $result[]="q{$number}_{$contract}";
  }
 }
 return $result;
}
function questionnaire_programs(): array {
 return [
  'PROPRIO'=>'Sim, possui programa habitacional próprio com recursos exclusivos do município',
  'ESTADUAL'=>'Não, atua somente com o Governo de Goiás no Programa Pra Ter Onde Morar',
  'FEDERAL'=>'Não, atua somente com o Governo Federal em empreendimentos do Minha Casa, Minha Vida',
  'AMBOS'=>'Não, atua com os programas Pra Ter Onde Morar e Minha Casa, Minha Vida',
 ];
}
/** Retorna [erros, dadosNormalizados]. */
function validate_questionnaire(array $input,array $municipalities):array {
 $errors=[];$answer=[];
 $municipality=$input['municipio']??'';
 $municipality=is_scalar($municipality)?trim((string)$municipality):'';
 if(!preg_match('/^52[0-9]{5}$/D',$municipality)||!array_key_exists((int)$municipality,$municipalities)){
  $errors['municipio']='Selecione um município válido de Goiás.';
 } else {$answer['municipio']=(int)$municipality;}
 foreach(questionnaire_fields() as $field){
  $raw=$input[$field]??null;
  $value=is_scalar($raw)?trim((string)$raw):'';
  if(!preg_match('/^(0|[1-9][0-9]{0,5})$/D',$value)||(float)$value>100000){
   $errors[$field]='Informe um número inteiro de 0 a 100.000.';
  } else {$answer[$field]=(int)$value;}
 }
 $program=$input['programa']??'';
 $program=is_string($program)?trim($program):'';
 if(!array_key_exists($program,questionnaire_programs())){
  $errors['programa']='Selecione uma das opções da questão 6.';
 } else {$answer['programa']=$program;}
 $details=$input['programa_detalhes']??'';
 $details=is_string($details)?trim($details):'';
 $length=function_exists('mb_strlen')?mb_strlen($details,'UTF-8'):strlen($details);
 if($program==='PROPRIO'&&($length<3||$length>2000)){
  $errors['programa_detalhes']='Informe o nome do programa e o benefício concedido (até 2.000 caracteres).';
 } elseif($program!=='PROPRIO'){$details='';}
 $answer['programa_detalhes']=$details;
 return [$errors,$answer];
}
