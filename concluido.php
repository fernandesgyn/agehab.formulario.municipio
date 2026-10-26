<?php
declare(strict_types=1);
require __DIR__.'/src/bootstrap.php';
$protocol=$_SESSION['success_protocol']??null;
unset($_SESSION['success_protocol']);
if(!is_int($protocol)||$protocol<=0)go('index.php');
$title='Formulário enviado';$active='formulario';
require __DIR__.'/partials/header.php';
?>
<div class="row justify-content-center"><div class="col-lg-7">
<div class="card text-center"><div class="card-body py-5">
 <div class="success-icon mb-3"><i class="bi bi-check-circle-fill"></i></div>
 <h4 class="mb-2">Informações registradas com sucesso</h4>
 <p class="text-muted">As respostas do seu município foram encaminhadas ao banco de dados da AGEHAB.</p>
 <div class="protocol-box mb-4">Protocolo de envio <strong>#<?= $protocol ?></strong></div>
 <div><a href="index.php" class="btn btn-outline-secondary">Voltar ao formulário</a></div>
</div></div></div></div>
<?php require __DIR__.'/partials/footer.php'; ?>
