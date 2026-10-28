(function () {
    'use strict';
    const form = document.getElementById('questionnaire-form');
    if (!form) return;

    const campos = [...form.querySelectorAll('.count-input')];
    function atualizarTotais() {
        form.querySelectorAll('.area-total').forEach(el => {
            const total = campos.filter(c => c.dataset.area === el.dataset.total)
                .reduce((n, c) => n + (parseInt(c.value, 10) || 0), 0);
            el.textContent = total.toLocaleString('pt-BR');
        });
    }
    campos.forEach(el => el.addEventListener('input', atualizarTotais));
    atualizarTotais();

    const grupoPrograma = document.getElementById('programa-detalhes-group');
    const detalhePrograma = document.getElementById('Dados_Detalhes');
    function atualizarPrograma() {
        const opcao = form.querySelector('input[name="Dados.Programa"]:checked');
        const temPrograma = opcao && opcao.value === 'PROPRIO';
        grupoPrograma.hidden = !temPrograma;
        detalhePrograma.disabled = !temPrograma;
        detalhePrograma.required = !!temPrograma;
        if (!temPrograma) detalhePrograma.value = '';
    }
    form.querySelectorAll('input[name="Dados.Programa"]').forEach(
        radio => radio.addEventListener('change', atualizarPrograma));
    atualizarPrograma();

    const estrutura = document.getElementById('Dados_Estrutura');
    const grupoEstrutura = document.getElementById('estrutura-outra-group');
    const outraEstrutura = document.getElementById('Dados_EstruturaOutra');
    function atualizarEstrutura() {
        const outra = estrutura.value === 'OUTRA';
        grupoEstrutura.hidden = !outra;
        outraEstrutura.disabled = !outra;
        outraEstrutura.required = outra;
        if (!outra) outraEstrutura.value = '';
    }
    estrutura.addEventListener('change', atualizarEstrutura);
    atualizarEstrutura();

    form.addEventListener('submit', e => {
        if (!form.checkValidity()) {
            e.preventDefault();
            form.classList.add('was-validated');
            form.querySelector(':invalid')?.focus();
            return;
        }

        for (const numero of [2, 3]) {
            const total = campos.filter(c => c.dataset.area === String(numero))
                .reduce((n, c) => n + (parseInt(c.value, 10) || 0), 0);
            const especifico = document.getElementById(
                numero === 2 ? 'q2_cress' : 'q3_exclusivos');
            if (Number(especifico.value) > total) {
                e.preventDefault();
                especifico.setCustomValidity('A quantidade não pode superar a soma dos vínculos.');
                especifico.reportValidity();
                especifico.addEventListener('input', () => especifico.setCustomValidity(''), {once: true});
                return;
            }
        }

        const btn = document.getElementById('send-form');
        btn.disabled = true;
        btn.textContent = 'Enviando...';
    });
})();
