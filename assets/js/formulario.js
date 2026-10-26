(function(){
 'use strict';
 const form=document.getElementById('questionnaire-form'); if(!form)return;
 const counts=[...document.querySelectorAll('.count-input')];
 const update=()=>document.querySelectorAll('.area-total').forEach(el=>{
  const n=counts.filter(e=>e.dataset.area===el.dataset.total).reduce((sum,e)=>sum+(parseInt(e.value,10)||0),0);
  el.textContent=n.toLocaleString('pt-BR');
 });
 counts.forEach(e=>e.addEventListener('input',update));update();
 const grp=document.getElementById('programa-detalhes-group'),det=document.getElementById('programa_detalhes');
 const show=()=>{const chosen=document.querySelector('input[name="programa"]:checked');
  const own=chosen&&chosen.value==='PROPRIO';grp.hidden=!own;det.disabled=!own;det.required=!!own;if(!own)det.value='';
 };
 document.querySelectorAll('input[name="programa"]').forEach(el=>el.addEventListener('change',show));show();
 form.addEventListener('submit',ev=>{
  if(!form.checkValidity()){ev.preventDefault();form.classList.add('was-validated');form.querySelector(':invalid')?.focus();return}
  const btn=document.getElementById('send-form');btn.disabled=true;btn.textContent='Enviando...';
 });
})();
