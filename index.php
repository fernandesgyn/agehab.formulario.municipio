<?php
declare(strict_types=1);
require __DIR__.'/src/bootstrap.php';
$title='Formulário para Municípios Goianos';$active='formulario';
$errors=$_SESSION['form_errors']??[];$old=$_SESSION['form_old']??[];
unset($_SESSION['form_errors'],$_SESSION['form_old']);
if(!is_array($errors))$errors=[];
if(!is_array($old))$old=[];
$areas=questionnaire_areas();$contracts=questionnaire_contracts();$programs=questionnaire_programs();
require __DIR__.'/partials/header.php';
?>
<div class="row justify-content-center"><div class="col-xl-10 col-xxl-9">
<div class="card mb-4"><div class="card-body form-intro d-flex flex-column flex-md-row gap-3 align-items-md-center">
 <div class="form-intro-icon"><i class="bi bi-buildings"></i></div><div>
 <h4 class="mb-1">Levantamento de equipes e programas habitacionais</h4>
 <p class="mb-0 text-muted">Preencha as informações do município. Nos quantitativos, informe zero quando não houver profissionais na categoria.</p>
 </div></div></div>
<?php if($errors): ?>
 <div class="alert alert-danger" role="alert"><strong>Não foi possível enviar o formulário.</strong>
 <p class="mb-0">Confira os campos destacados. <?= h($errors['geral']??'') ?></p></div>
<?php endif; ?>
<form method="post" action="salvar.php" id="questionnaire-form" novalidate>
<input type="hidden" name="_csrf" value="<?= h(csrf()) ?>">
<section class="card mb-4"><div class="card-header"><h5 class="card-title mb-0">1. Identificação do município</h5></div>
<div class="card-body">
 <label for="municipio" class="form-label fw-semibold">Nome do município <span class="text-danger">*</span></label>
 <select class="form-select <?= isset($errors['municipio'])?'is-invalid':'' ?>" id="municipio" name="municipio" required>
 <option value="">Selecione um município de Goiás...</option>
 <?php foreach(municipalities() as $ibge=>$municipality): ?>
 <option value="<?= $ibge ?>" <?= (string)($old['municipio']??'')===(string)$ibge?'selected':'' ?>><?= h($municipality) ?></option>
 <?php endforeach; ?>
 </select>
 <div class="invalid-feedback"><?= h($errors['municipio']??'Selecione um município.') ?></div>
 <div class="form-text">Seleção entre os 246 municípios do Estado de Goiás.</div>
</div></section>
<?php foreach($areas as $number=>$area): ?>
<section class="card mb-4">
 <div class="card-header d-flex align-items-center justify-content-between gap-2">
  <h5 class="card-title mb-0"><?= $number ?>. <?= h($area) ?></h5><span class="badge text-bg-light border">Questão <?= $number ?></span>
 </div>
 <div class="card-body"><p class="text-muted small">Informe a quantidade de profissionais por tipo de vínculo.</p>
 <div class="row g-3">
 <?php $index=0;foreach($contracts as $suffix=>$label):$index++;$field="q{$number}_{$suffix}"; ?>
  <div class="col-12 col-sm-6">
   <label for="<?= h($field) ?>" class="form-label"><?= $number ?>.<?= $index ?> <?= h($label) ?></label>
   <input type="number" inputmode="numeric" min="0" max="100000" step="1"
    class="form-control count-input <?= isset($errors[$field])?'is-invalid':'' ?>"
    name="<?= h($field) ?>" id="<?= h($field) ?>" data-area="<?= $number ?>"
    value="<?= h(is_scalar($old[$field]??null)?$old[$field]:'0') ?>" required>
   <div class="invalid-feedback"><?= h($errors[$field]??'Informe um número inteiro de 0 a 100.000.') ?></div>
  </div>
 <?php endforeach; ?>
 </div></div>
 <div class="card-footer bg-body-tertiary d-flex justify-content-between align-items-center">
  <span class="text-muted">Total de profissionais informados neste bloco</span>
  <strong class="area-total" data-total="<?= $number ?>">0</strong>
 </div>
</section>
<?php endforeach; ?>
<section class="card mb-4"><div class="card-header"><h5 class="card-title mb-0">6. Programas habitacionais do município</h5></div>
<div class="card-body"><p class="mb-3">O município possui algum programa habitacional próprio, entendido como programa financiado exclusivamente com recursos municipais?</p>
<?php foreach($programs as $code=>$label): ?>
<div class="form-check mb-3">
 <input class="form-check-input <?= isset($errors['programa'])?'is-invalid':'' ?>" type="radio"
  name="programa" id="programa_<?= h($code) ?>" value="<?= h($code) ?>"
  <?= ($old['programa']??'')===$code?'checked':'' ?> required>
 <label class="form-check-label" for="programa_<?= h($code) ?>"><?= h($label) ?></label>
</div>
<?php endforeach; ?>
<?php if(isset($errors['programa'])): ?><p class="small text-danger"><?= h($errors['programa']) ?></p><?php endif; ?>
<div id="programa-detalhes-group" class="mt-3 p-3 rounded border bg-body-tertiary" hidden>
 <label class="form-label fw-semibold" for="programa_detalhes">6.1. Informe o nome do programa próprio e o benefício concedido <span class="text-danger">*</span></label>
 <textarea class="form-control <?= isset($errors['programa_detalhes'])?'is-invalid':'' ?>"
 id="programa_detalhes" name="programa_detalhes" rows="4" maxlength="2000"
 placeholder="Ex.: nome do programa; concessão de lotes, casas ou ajuda de custo."><?= h(is_scalar($old['programa_detalhes']??null)?$old['programa_detalhes']:'') ?></textarea>
 <div class="invalid-feedback"><?= h($errors['programa_detalhes']??'Informe o nome e o benefício.') ?></div>
</div>
</div></section>
<div class="d-flex flex-column flex-sm-row justify-content-between align-items-sm-center gap-3 pb-4">
 <span class="small text-muted"><i class="bi bi-shield-check me-1"></i>As informações serão registradas no banco da AGEHAB.</span>
 <button type="submit" class="btn btn-success px-4" id="send-form"><i class="bi bi-check2-circle me-1"></i>Enviar formulário</button>
</div>
</form></div></div>
<script src="assets/js/formulario.js"></script>
<?php require __DIR__.'/partials/footer.php'; ?>
