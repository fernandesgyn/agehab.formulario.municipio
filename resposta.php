<?php
declare(strict_types=1);
require __DIR__.'/src/bootstrap.php';require_admin();
$id=filter_input(INPUT_GET,'id',FILTER_VALIDATE_INT);
if(!$id||$id<1){http_response_code(400);exit('Identificador inválido.');}
try{
 $statement=database()->prepare('SELECT R.*, M.Nome AS Municipio FROM dbo.RespostasMunicipais R JOIN dbo.Municipios M ON M.CodigoIBGE=R.CodigoIBGE WHERE R.Id=?');
 $statement->execute([$id]);$record=$statement->fetch();
 if(!$record){http_response_code(404);exit('Resposta não localizada.');}
}catch(Throwable $exception){
 error_log('Consulta resposta municipal: '.$exception->getMessage());
 http_response_code(503);exit('Não foi possível consultar os dados.');
}
$title='Resposta municipal #'.$id;$active='admin';require __DIR__.'/partials/header.php';
?>
<div class="row justify-content-center"><div class="col-xl-9">
<div class="mb-3"><a href="admin.php" class="btn btn-outline-secondary btn-sm"><i class="bi bi-arrow-left"></i> Voltar</a></div>
<section class="card mb-4"><div class="card-header"><h5 class="card-title mb-0">Município: <?= h($record['Municipio']) ?></h5></div>
<div class="card-body"><span class="text-muted">Código IBGE: <?= (int)$record['CodigoIBGE'] ?> • Protocolo #<?= (int)$record['Id'] ?></span></div></section>
<?php foreach(questionnaire_areas() as $number=>$area): ?>
<section class="card mb-4"><div class="card-header"><h5 class="card-title mb-0"><?= $number ?>. <?= h($area) ?></h5></div>
<div class="card-body"><dl class="row mb-0">
<?php foreach(questionnaire_contracts() as $key=>$label): ?>
<dt class="col-sm-8"><?= h($label) ?></dt><dd class="col-sm-4"><?= (int)$record[strtoupper("q{$number}_{$key}")] ?></dd>
<?php endforeach; ?>
</dl></div></section>
<?php endforeach; ?>
<section class="card mb-4"><div class="card-header"><h5 class="card-title mb-0">6. Programas habitacionais</h5></div>
<div class="card-body"><p><?= h(questionnaire_programs()[$record['ProgramaHabitacionalTipo']]??'') ?></p>
<?php if($record['ProgramaHabitacionalTipo']==='PROPRIO'): ?>
<div class="p-3 border rounded bg-body-tertiary" style="white-space:pre-wrap"><?= h($record['ProgramaProprioDetalhes']??'') ?></div>
<?php endif; ?>
</div></section></div></div>
<?php require __DIR__.'/partials/footer.php'; ?>
