<?php
declare(strict_types=1);
require dirname(__DIR__).'/src/questionnaire.php';
$m=require dirname(__DIR__).'/data/municipios.php';
assert(count($m)===246);
assert(($m[5208707]??null)==='Goiânia');
assert(($m[5200134]??null)==='Acreúna');
assert(count(questionnaire_fields())===16);
$valid=['municipio'=>'5208707','programa'=>'PROPRIO','programa_detalhes'=>'Programa Municipal - lotes'];
foreach(questionnaire_fields() as $f)$valid[$f]='0';
[$errors,$answer]=validate_questionnaire($valid,$m);
assert($errors===[]);
assert($answer['municipio']===5208707);
$valid['municipio']='9999999';$valid['q2_efetivos']='-1';$valid['programa_detalhes']='';
[$errors]=validate_questionnaire($valid,$m);
assert(isset($errors['municipio'],$errors['q2_efetivos'],$errors['programa_detalhes']));
echo "OK: 246 municípios e validação de respostas.\n";
