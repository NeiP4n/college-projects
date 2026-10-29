#include <fstream>
#include <cstdint>

void createFakeAudioFile(const char* filename) {
    std::ofstream file(filename, std::ios::binary);

    // WAV-заголовок (с поддельной длительностью)
    char riffHeader[44] = {
        'R', 'I', 'F', 'F',  // ChunkID
        0xFF, 0xFF, 0xFF, 0xFF,  // ChunkSize (максимальное значение)
        'W', 'A', 'V', 'E',  // Format
        'f', 'm', 't', ' ',  // Subchunk1ID
        16, 0, 0, 0,  // Subchunk1Size (16 для PCM)
        1, 0,  // AudioFormat (1 = PCM)
        2, 0,  // NumChannels (2 = стерео)
        0x44, 0xAC, 0x00, 0x00,  // SampleRate (44100 Гц)
        0x10, 0xB1, 0x02, 0x00,  // ByteRate (44100 * 2 * 2)
        4, 0,  // BlockAlign (2 байта на канал)
        16, 0,  // BitsPerSample (16 бит)
        'd', 'a', 't', 'a',  // Subchunk2ID
        0xFF, 0xFF, 0xFF, 0xFF  // Subchunk2Size (максимальное значение)
    };

    file.write(riffHeader, 44);
    file.close();
}

int main() {
    createFakeAudioFile("fake_audio.wav");
    return 0;
}
