<?php
/** @var string $title */
/** @var string $active */
$appName=(string)(getenv('APP_NAME')?:'Formulário dos Municípios Goianos');
?>
<!doctype html>
<html lang="pt-BR" data-bs-theme="light">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<meta name="color-scheme" content="light">
<title><?= h($title) ?> | AGEHAB</title>
<link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.13.1/font/bootstrap-icons.min.css" rel="stylesheet">
<link href="https://cdn.jsdelivr.net/npm/admin-lte@4.10.0/dist/css/adminlte.min.css" rel="stylesheet">
<link href="assets/css/admin.css" rel="stylesheet">
<link href="assets/css/adminlte-overrides.css" rel="stylesheet">
<link href="assets/css/formulario.css" rel="stylesheet">
</head>
<body class="layout-fixed sidebar-expand-lg bg-body-tertiary portal-admin-body">
<div class="app-wrapper">
<nav class="app-header navbar navbar-expand bg-body border-bottom">
 <div class="container-fluid"><ul class="navbar-nav">
 <li class="nav-item"><a class="nav-link" data-lte-toggle="sidebar" href="#" role="button" aria-label="Abrir menu"><i class="bi bi-list"></i></a></li>
 <li class="nav-item d-none d-sm-block"><span class="nav-link header-system-name"><?= h($appName) ?></span></li>
 </ul><span class="navbar-text ms-auto small text-muted">AGEHAB • Goiás</span></div>
</nav>
<aside class="app-sidebar bg-body-secondary shadow" data-bs-theme="light">
 <div class="sidebar-brand"><a href="index.php" class="brand-link d-flex align-items-center gap-2 text-decoration-none">
 <img src="assets/img/logo-agehab.svg" width="66" height="40" alt="AGEHAB"><span class="brand-text fw-semibold small">Formulário Municipal</span>
 </a></div>
 <div class="sidebar-wrapper"><nav class="mt-2"><ul class="nav sidebar-menu flex-column" data-lte-toggle="treeview" role="menu">
 <li class="nav-item"><a href="index.php" class="nav-link <?= $active==='formulario'?'active':'' ?>"><i class="nav-icon bi bi-ui-checks"></i><p>Formulário</p></a></li>
 <li class="nav-item"><a href="admin.php" class="nav-link <?= $active==='admin'?'active':'' ?>"><i class="nav-icon bi bi-table"></i><p>Área administrativa</p></a></li>
 </ul></nav></div>
</aside>
<main class="app-main"><div class="app-content-header"><div class="container-fluid">
 <h3 class="mb-1"><?= h($title) ?></h3><p class="text-muted mb-0 small">Agência Goiana de Habitação — AGEHAB</p>
</div></div><div class="app-content"><div class="container-fluid">
