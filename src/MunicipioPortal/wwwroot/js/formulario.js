(function() {
 'use strict';
 const form=document.getElementById('questionnaire-form');if(!form)return;
 const nums=[...document.querySelectorAll('.count-input')];
 function calcular() {
  document.querySelectorAll('.area-total').forEach(el=>{
   const total=nums.filter(n=>n.dataset.area===el.dataset.total)
     .reduce((ac,n)=>ac+(parseInt(n.value,10)||0),0);
   el.textContent=total.toLocaleString('pt-BR');
  });
 }
 nums.forEach(el=>el.addEventListener('input',calcular));calcular();
 const group=document.getElementById('programa-detalhes-group'),det=document.getElementById('Dados_Detalhes');
 function atualizar() {
  const selected=document.querySelector('input[name="Dados.Programa"]:checked');
  const own=selected?.value==='PROPRIO';
  group.hidden=!own;det.disabled=!own;det.required=!!own;
  if(!own)det.value='';
 }
 document.querySelectorAll('input[name="Dados.Programa"]').forEach(el=>el.addEventListener('change',atualizar));
 atualizar();
 form.addEventListener('submit',ev=>{
  if(!form.checkValidity()) {
   ev.preventDefault();form.classList.add('was-validated');form.querySelector(':invalid')?.focus();return;
  }
  const btn=document.getElementById('send-form');btn.disabled=true;btn.textContent='Enviando...';
 });
})();
