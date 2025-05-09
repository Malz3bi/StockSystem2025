$(document).ready(function () {
    // Destroy existing DataTable instance if it exists
    if ($.fn.dataTable.isDataTable('#datanew')) {
        $('#datanew').DataTable().destroy();
    }

    // Initialize DataTable
    $('#datanew').DataTable({
        "language": {
            "sProcessing": "جاري المعالجة...",
            "sLengthMenu": "عرض _MENU_ مدخلات",
            "sZeroRecords": "لم يتم العثور على نتائج",
            "sEmptyTable": "لا توجد بيانات متاحة في الجدول",
            "sInfo": "إظهار _START_ إلى _END_ من _TOTAL_ مدخلات",
            "sInfoEmpty": "إظهار 0 إلى 0 من 0 مدخلات",
            "sInfoFiltered": "(تمت تصفيتها من _MAX_ مدخلات كليّة)",
            "sSearch": "بحث:",
            "sLoadingRecords": "جاري التحميل...",
            "oPaginate": {
                "sFirst": "الأول",
                "sPrevious": "السابق",
                "sNext": "التالي",
                "sLast": "الأخير"
            },
            "oAria": {
                "sSortAscending": ": تفعيل لترتيب العمود تصاعديًا",
                "sSortDescending": ": تفعيل لترتيب العمود تنازليًا"
            }
        },
        "destroy": true // Ensure reinitialization is allowed
    });
});

