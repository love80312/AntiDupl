/*
* AntiDupl.NET Program (http://ermig1979.github.io/AntiDupl).
*
* Copyright (c) 2002-2018 Yermalayeu Ihar, 2013-2018 Borisov Dmitry.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy 
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Text;

namespace AntiDupl.NET.WinForms
{
    static public class StringsDefaultRussian
    {
        static public void CopyTo(Strings s)
        {
            s.Name = "Russian";
            s.OriginalLanguageName = "Русский";

            s.OkButton_Text = "OK";
            s.CancelButton_Text = "Отмена";
            s.StopButton_Text = "Стоп";
            s.SetDefaultButton_Text = "По умолчанию";

            s.ErrorMessage_FileAlreadyExists = "Невозможно переименовать файл, так как файл с таким именем уже существует!";
            s.MainForm_SearchFoldersNotFound = "Эти папки из командной строки не существуют и не включены в поиск:\n{0}";
            s.MainForm_IgnoreFoldersNotFound = "Эти папки для исключения из командной строки не существуют и пропущены:\n{0}";
            s.ErrorMessage_SettingsFileUnreadable = "Не удалось прочитать файл настроек\n{0}\n\n{1}\n\nВместо него используются настройки по умолчанию; при закрытии программы они будут сохранены в этот файл. Копия нечитаемого файла сохранена как\n{2}";
            s.ErrorMessage_SettingsFileUnreadableNotCopied = "Не удалось прочитать файл настроек\n{0}\n\n{1}\n\nВместо него используются настройки по умолчанию; при закрытии программы они будут сохранены в этот файл. Скопировать файл не удалось: чтобы сохранить его, скопируйте его в другое место до закрытия программы.";

            s.WarningMessage_ChangeFileExtension = "Вы действительно хотите изменить расширение файла?";

            s.AboutProgramPanel_CopyrightLabel0_Text = "Copyright (c) 2002-2023 Ермолаев Игорь,";
            s.AboutProgramPanel_CopyrightLabel1_Text = "2013-2023 Борисов Дмитрий,";
            s.AboutProgramPanel_CopyrightLabel2_Text = "2023-2023 Edi61.";
            s.AboutProgramPanel_ComponentLabel_Text = "Компонент";
            s.AboutProgramPanel_VersionLabel_Text = "Версия";

            s.AboutProgramForm_Text = "О программе...";

            s.StartFinishForm_LoadImages_Text = "Загрузка файла картинок";
            s.StartFinishForm_LoadMistakes_Text = "Загрузка файла ошибок";
            s.StartFinishForm_LoadResults_Text = "Загрузка файла результатов";
            s.StartFinishForm_SaveImages_Text = "Сохранение файла картинок";
            s.StartFinishForm_SaveMistakes_Text = "Сохранение файла ошибок";
            s.StartFinishForm_SaveResults_Text = "Сохранение файла результатов";
            s.StartFinishForm_ClearTemporary_Text = "Удаление временных файлов";

            s.CoreOptionsForm_Text = "Опции";

            s.CoreOptionsForm_SearchTabPage_Text = "Поиск";
            s.CoreOptionsForm_SearchFileTypeGroupBox_Text = "Искать типы:";
            s.CoreOptionsForm_BmpCheckBox_Text = "BMP";
            s.CoreOptionsForm_GifCheckBox_Text = "GIF";
            s.CoreOptionsForm_JpegCheckBox_Text = "JPEG";
            s.CoreOptionsForm_PngCheckBox_Text = "PNG";
            s.CoreOptionsForm_TiffCheckBox_Text = "TIFF";
            s.CoreOptionsForm_EmfCheckBox_Text = "EMF";
            s.CoreOptionsForm_WmfCheckBox_Text = "WMF";
            s.CoreOptionsForm_ExifCheckBox_Text = "EXIF";
            s.CoreOptionsForm_IconCheckBox_Text = "ICON";
            s.CoreOptionsForm_Jp2CheckBox_Text = "JP2";
            s.CoreOptionsForm_PsdCheckBox_Text = "PSD";
            s.CoreOptionsForm_DdsCheckBox_Text = "DDS";
            s.CoreOptionsForm_TgaCheckBox_Text = "TGA";
            s.CoreOptionsForm_WebpCheckBox_Text = "WEBP";
            s.CoreOptionsForm_HeifCheckBox_Text = "HEIF";
            s.CoreOptionsForm_AvifCheckBox_Text = "AVIF";
            s.CoreOptionsForm_JxlCheckBox_Text = "JXL";
            s.CoreOptionsForm_SearchSystemCheckBox_Text = "Искать системные каталоги/файлы";
            s.CoreOptionsForm_SearchHiddenCheckBox_Text = "Искать скрытые каталоги/файлы";

            s.CoreOptionsForm_CompareTabPage_Text = "Сравнение";
            s.CoreOptionsForm_CheckOnEqualityCheckBox_Text = "Искать дублирующие картинки";
            s.CoreOptionsForm_TransformedImageCheckBox_Text = "Искать повернутые и зеркальные дубликаты";
            s.CoreOptionsForm_ShiftedImageCheckBox_Text = "Искать дубликаты со сдвигом изображения";
            s.CoreOptionsForm_SizeControlCheckBox_Text = "Учитывать размер картинок";
            s.CoreOptionsForm_TypeControlCheckBox_Text = "Учитывать тип картинок";
            s.CoreOptionsForm_RatioControlCheckBox_Text = "Учитывать соотношение ширины/высоты картинок";
            s.CoreOptionsForm_AlgorithmComparingLabeledComboBox_Text = "Алгоритм сравнения изображений";
            s.CoreOptionsForm_AlgorithmComparingLabeledComboBox_SquaredSum = "Среднеквадратичная разность";
            s.CoreOptionsForm_AlgorithmComparingLabeledComboBox_LocalColor = "Локальная разность цвета";
            s.CoreOptionsForm_ThresholdDifferenceLabeledComboBox_Text = "Пороговая разность";
            s.CoreOptionsForm_MinimalImageSizeLabeledIntegerEdit_Text = "Минимальная ширина/высота картинок";
            s.CoreOptionsForm_MaximalImageSizeLabeledIntegerEdit_Text = "Максимальная ширина/высота картинок";
            s.CoreOptionsForm_CompareInsideOneSearchPathCheckBox_Text = "Сравнивать картинки из одного пути поиска друг с другом";
            s.CoreOptionsForm_SkipHardlinksCheckBox_Text = "Не сравнивать жесткие ссылки на один файл";
            s.CoreOptionsForm_CompareInsideOneFolderCheckBox_Text = "Сравнивать картинки внутри одного каталога";

            s.CoreOptionsForm_DefectTabPage_Text = "Дефекты";
            s.CoreOptionsForm_CheckOnDefectCheckBox_Text = "Проверять на дефекты";
            s.CoreOptionsForm_CheckOnBlockinessCheckBox_Text = "Проверять на блочность";
            s.CoreOptionsForm_BlockinessThresholdLabeledComboBox_Text = "Порог блочности";
            s.CoreOptionsForm_CheckOnBlockinessOnlyNotJpegCheckBox_Text = "Проверять на блочность только для не Jpeg";
            s.CoreOptionsForm_CheckOnBlurringCheckBox_Text = "Проверять на размытость";
            s.CoreOptionsForm_BlurringThresholdLabeledComboBox_Text = "Порог размытости";

            s.CoreOptionsForm_AdvancedTabPage_Text = "Дополнительные";
            s.CoreOptionsForm_DeleteToRecycleBinCheckBox_Text = "Удалять в корзину";
            s.CoreOptionsForm_MistakeDataBaseCheckBox_Text = "Запоминать ложные срабатывания";
            s.CoreOptionsForm_RatioResolutionLabeledComboBox_Text = "Точность соотношения ширины/высоты картинок";
            s.CoreOptionsForm_CompareThreadCountLabeledComboBox_Text = "Количество потоков сравнения";
            s.CoreOptionsForm_CompareThreadCountLabeledComboBox_Description_0 = "Авто";
            s.CoreOptionsForm_CollectThreadCountLabeledComboBox_Text = "Количество потоков загрузки";
            s.CoreOptionsForm_CollectThreadCountLabeledComboBox_Description_0 = "Авто";
            s.CoreOptionsForm_ReducedImageSizeLabeledComboBox_Text = "Отнормированный размер картинок";
            s.CoreOptionsForm_UndoQueueSizeLabeledIntegerEdit_Text = "Размер очереди отмены";
            s.CoreOptionsForm_ResultCountMaxLabeledIntegerEdit_Text = "Максимальное количество результатов";
            s.CoreOptionsForm_IgnoreFrameWidthLabeledComboBox_Text = "Ширина игнорируемой рамки картинки";
            s.CoreOptionsForm_UseLibJpegTurboCheckBox_Text = "Использовать libjpeg-turbo";

            s.CoreOptionsForm_HighlightTabPage_Text = "Подсветка";
            s.CoreOptionsForm_HighlightDifferenceCheckBox_Text = "Подсветка различий";
            s.CoreOptionsForm_DifrentValue_Text = "Величина различия";
            s.CoreOptionsForm_NotHighlightIfFragmentsMoreThemCheckBox_Text = "Не подсвечивать различия, если фрагментов больше чем:";
            s.CoreOptionsForm_MaxFragmentsForDisableHighlightLabeledIntegerEdit_Text = "Максимально количество фрагментов для отключения подсветки";
            s.CoreOptionsForm_HighlightAllDifferencesCheckBox_Text = "Подсвечивать все различия";
            s.CoreOptionsForm_MaxFragmentsForHighlightLabeledIntegerEdit_Text = "Максимально количество фрагментов для подсветки";
            s.CoreOptionsForm_AmountOfFragmentsOnXLabeledIntegerEdit_Text =
                
