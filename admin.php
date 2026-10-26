<?php
declare(strict_types=1);
require __DIR__.'/src/bootstrap.php';
$loginError='';
if($_SERVER['REQUEST_METHOD']==='POST'){
 if(!verify_csrf()){http_response_code(419);exit('Sessão inválida. Atualize a página.');}
 if(isset($_POST['logout'])){
  unset($_SESSION['admin_authenticated']);session_regenerate_id(true);go('admin.php');
 }
 $hash=trim((string)(getenv('ADMIN_PASSWORD_HASH')?:''));
 $password=(string)($_POST['senha']??'');
 $failed=(int)($_SESSION['admin_failed']??0);
 if($failed>=10){
  $loginError='Limite de tentativas alcançado nesta sessão.';
 } elseif($hash!==''&&password_verify($password,$hash)){
  session_regenerate_id(true);
  unset($_SESSION['admin_failed']);
  $_SESSION['admin_authenticated']=true;go('admin.php');
 }else{
  $_SESSION['admin_failed']=$failed+1;
  $loginError=$hash===''?'Acesso administrativo não configurado no servidor.':'Senha incorreta.';
 }
}
$title='Área administrativa';$active='admin';
if(empty($_SESSION['admin_authenticated'])){
 require __DIR__.'/partials/header.php';
 ?>
 <div class="row justify-content-center"><div class="col-md-6 col-lg-4">
 <div class="card"><div class="card-header"><h5 class="card-title mb-0">Acesso restrito</h5></div>
 <div class="card-body"><?php if($loginError): ?><div class="alert alert-danger"><?= h($loginError) ?></div><?php endif; ?>
 <form method="post"><input type="hidden" name="_csrf" value="<?= h(csrf()) ?>">
 <label for="senha" class="form-label">Senha de administração</label>
 <input type="password" id="senha" name="senha" class="form-control mb-3" autocomplete="current-password" required>
 <button class="btn btn-success w-100" type="submit">Entrar</button>
 </form></div></div></div></div>
 <?php require __DIR__.'/partials/footer.php';exit;
}
$error=null;$records=[];
try{
 $cols=implode(', ',array_map(static fn(string $f):string=>'R.'.strtoupper($f),questionnaire_fields()));
 $sql="SELECT R.Id, M.Nome AS Municipio, R.CriadoEm, R.ProgramaHabitacionalTipo, {$cols}
 FROM dbo.RespostasMunicipais R JOIN dbo.Municipios M ON M.CodigoIBGE=R.CodigoIBGE ORDER BY M.Nome";
 $records=database()->query($sql)->fetchAll();
}catch(Throwable $exception){
 error_log('Admin formulario AGEHAB: '.$exception->getMessage());
 $error='Não foi possível consultar as respostas no SQL Server.';
}
require __DIR__.'/partials/header.php';
?>
<?php if($error): ?><div class="alert alert-danger"><?= h($error) ?></div><?php endif; ?>
<div class="card">
<div class="card-header d-flex flex-wrap align-items-center gap-3">
 <h5 class="card-title mb-0">Municípios respondentes: <?= count($records) ?> de 246</h5>
 <div class="ms-auto d-flex flex-wrap gap-2">
 <a href="exportar.php" class="btn btn-outline-success btn-sm"><i class="bi bi-file-earmark-spreadsheet"></i> Exportar CSV</a>
 <form method="post" class="m-0"><input type="hidden" name="_csrf" value="<?= h(csrf()) ?>"><button type="submit" name="logout" value="1" class="btn btn-outline-secondary btn-sm">Sair</button></form>
 </div>
</div>
<div class="card-body">
 <label for="filtro" class="form-label">Pesquisar município</label>
 <input type="search" class="form-control mb-3" id="filtro" placeholder="Digite o nome do município">
 <div class="table-responsive"><table class="table table-striped table-hover align-middle" id="tabela-respostas">
 <thead><tr><th>Município</th><th>Cadastro/seleção</th><th>Visitas sociais</th><th>TS pré</th><th>TS pós</th><th>Programa</th><th>Enviado em</th><th>Ação</th></tr></thead>
 <tbody>
 <?php foreach($records as $record): ?>
 <tr><td><?= h($record['Municipio']) ?></td>
 <?php foreach([2,3,4,5] as $n): ?>
 <td><?= array_sum(array_map(static fn(string $key):int=>(int)$record[strtoupper("q{$n}_{$key}")],array_keys(questionnaire_contracts()))) ?></td>
 <?php endforeach; ?>
 <td><?= h(questionnaire_programs()[$record['ProgramaHabitacionalTipo']]??$record['ProgramaHabitacionalTipo']) ?></td>
 <td><?= h($record['CriadoEm'] instanceof DateTimeInterface?$record['CriadoEm']->format('d/m/Y H:i'):$record['CriadoEm']) ?></td>
 <td><a class="btn btn-sm btn-outline-primary" href="resposta.php?id=<?= (int)$record['Id'] ?>">Consultar</a></td>
 </tr>
 <?php endforeach; ?>
 <?php if(!$records): ?><tr class="empty-row"><td colspan="8" class="text-muted">Nenhuma resposta encontrada.</td></tr><?php endif; ?>
 </tbody></table></div>
</div></div>
<script>
document.getElementById('filtro').addEventListener('input',function(){
 const search=this.value.toLocaleLowerCase('pt-BR');
 document.querySelectorAll('#tabela-respostas tbody tr:not(.empty-row)').forEach(tr=>{
 tr.hidden=!tr.cells[0].textContent.toLocaleLowerCase('pt-BR').includes(search);
 });
});
</script>
<?php require __DIR__.'/partials/footer.php'; ?>
